using System.Collections.Concurrent;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace Echo.Platforms.AsmResolver.Emulation.Runtime;

/// <summary>
/// Provides a mechanism for initialization and management of types residing in a virtual machine.
/// </summary>
public sealed class RuntimeTypeManager
{
    private readonly CilVirtualMachine _machine;
    
    private readonly ConcurrentDictionary<ITypeDescriptor, TypeInitialization> _initializations 
        = new(SignatureComparer.Default);

    /// <summary>
    /// Creates a new runtime type manager.
    /// </summary>
    /// <param name="machine">The machine the type is made for.</param>
    public RuntimeTypeManager(CilVirtualMachine machine)
    {
        _machine = machine;
    }

    private TypeInitialization GetInitialization(ITypeDescriptor type)
    {
        if (_initializations.TryGetValue(type, out var initialization))
            return initialization;
        
        var newInitialization = new TypeInitialization(type);
        while (!_initializations.TryGetValue(type, out initialization))
        {
            if (_initializations.TryAdd(type, newInitialization))
            {
                initialization = newInitialization;
                break;
            }
        }

        return initialization;
    }

    /// <summary>
    /// Registers the event that a type has failed to initialize. 
    /// </summary>
    /// <param name="type">The type that failed to initialize.</param>
    /// <param name="innerException">The exception object that describes the failure.</param>
    /// <returns>The resulting TypeInitializationException instance.</returns>
    public ObjectHandle RegisterInitializationException(ITypeDescriptor type, ObjectHandle innerException)
    {
        var initialization = GetInitialization(type);
        if (!initialization.Exception.IsNull)
            return initialization.Exception;

        lock (initialization)
        {
            if (initialization.Exception.IsNull)
            {
                initialization.Exception = _machine.Heap
                    .AllocateObject(_machine.ValueFactory.TypeInitializationExceptionType, true)
                    .AsObjectHandle(_machine);
            }
            
            // TODO: incorporate `innerException`.
        }

        return initialization.Exception;
    }
    
    /// <summary>
    /// Handles the type initialization on the provided thread.
    /// </summary>
    /// <param name="thread">The thread the initialization is to be called on.</param>
    /// <param name="type">The type to initialize.</param>
    /// <returns>The initialization result.</returns>
    public TypeInitializerResult HandleInitialization(CilThread thread, ITypeDescriptor type)
    {
        var initialization = GetInitialization(type);

        // If we already have an exception cached as a result of a previous type-load failure, rethrow it.
        if (!initialization.Exception.IsNull)
            return TypeInitializerResult.Exception(initialization.Exception);
        
        // We only need to call the constructor once.
        if (initialization.ConstructorCalled)
            return TypeInitializerResult.NoAction();
        
        lock (initialization)
        {
            // Try check if any thread beat us in the initialization handling.
            if (!initialization.Exception.IsNull)
                return TypeInitializerResult.Exception(initialization.Exception);

            if (initialization.ConstructorCalled)
                return TypeInitializerResult.NoAction();
            
            // Try resolve the type that is being initialized.
            var definition = type.Resolve();
            if (definition is null)
            {
                initialization.Exception = _machine.Heap
                    .AllocateObject(_machine.ValueFactory.TypeInitializationExceptionType, true)
                    .AsObjectHandle(_machine);
                    
                return TypeInitializerResult.Exception(initialization.Exception);
            }
            
            // "Call" the constructor.
            initialization.ConstructorCalled = true;
                
            // Actually find the constructor and call it if it is there.
            var cctor = definition.GetStaticConstructor();
            if (cctor is not null)
            {
                thread.CallStack.Push(cctor);
                return TypeInitializerResult.Redirected();
            }

            return TypeInitializerResult.NoAction();
        }
    }
 
    private sealed class TypeInitialization
    {
        public TypeInitialization(ITypeDescriptor type)
        {
            Type = type;
        }

        public ITypeDescriptor Type { get; }
        
        public bool ConstructorCalled { get; set; }

        public ObjectHandle Exception { get; set; }
    }
}