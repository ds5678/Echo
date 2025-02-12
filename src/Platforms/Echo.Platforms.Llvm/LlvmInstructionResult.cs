using Echo.Code;
using LLVMSharp.Interop;

namespace Echo.Platforms.Llvm;

/// <summary>
/// Represents a variable that is backed by the result of an LLVM instruction.
/// </summary>
public class LlvmInstructionResult : IVariable
{
    /// <inheritdoc />
    public string Name => Instruction.ToString();

    /// <summary>
    /// Gets the instruction that this variable represents.
    /// </summary>
    public LLVMValueRef Instruction { get; }

    /// <summary>
    /// Creates a new LLVM instruction variable.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    public LlvmInstructionResult(LLVMValueRef instruction) => Instruction = instruction;
}
