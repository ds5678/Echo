using Echo.Code;
using LLVMSharp.Interop;
using System;

namespace Echo.Platforms.Llvm;

/// <summary>
/// Represents a parameter that is declared and can be referenced within an LLVM function.
/// </summary>
public class LlvmParameter : IVariable
{
    /// <summary>
    /// Creates a new LLVM parameter.
    /// </summary>
    /// <param name="parameter">The underlying parameter</param>
    /// <param name="index">The index of the parameter.</param>
    public LlvmParameter(LLVMValueRef parameter, int index)
    {
        if (parameter.IsAArgument == default)
        {
            throw new ArgumentException("Provided value is not a parameter.", nameof(parameter));
        }
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Parameter = parameter;
        Index = index;
    }

    /// <summary>
    /// Gets the underlying parameter object.
    /// </summary>
    public LLVMValueRef Parameter
    {
        get;
    }

    /// <summary>
    /// Gets the index of the parameter.
    /// </summary>
    public int Index { get; }

    /// <inheritdoc />
    public string Name => "param_" + Index;

    /// <inheritdoc />
    public override string ToString() => Name;
}
