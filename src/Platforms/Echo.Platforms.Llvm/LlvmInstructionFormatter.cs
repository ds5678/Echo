using Echo.ControlFlow.Serialization.Dot;
using LLVMSharp.Interop;

namespace Echo.Platforms.Llvm;

/// <summary>
/// Provides a custom formatter for <see cref="LLVMValueRef"/>s.
/// </summary>
public class LlvmInstructionFormatter : IInstructionFormatter<LLVMValueRef>
{
    /// <inheritdoc />
    public string Format(in LLVMValueRef instruction) => instruction.PrintToString();
}