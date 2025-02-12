using LLVMSharp.Interop;
using System.Collections.Generic;

namespace Echo.Platforms.Llvm;

internal static class LlvmExtensions
{
    public static IEnumerable<LLVMValueRef> GetInstructions(this LLVMBasicBlockRef basicBlock)
    {
        var instruction = basicBlock.FirstInstruction;
        while (instruction.Handle != default)
        {
            yield return instruction;
            instruction = instruction.NextInstruction;
        }
    }

    public static IEnumerable<LLVMValueRef> GetOperands(this LLVMValueRef instruction)
    {
        int numOperands = instruction.OperandCount;
        for (int i = 0; i < numOperands; i++)
        {
            yield return GetOperand(instruction, i);
        }

        static unsafe LLVMValueRef GetOperand(LLVMValueRef instruction, int index)
        {
            return instruction.GetOperand((uint)index);
        }
    }

    public static FlowControl GetFlowControl(this in LLVMValueRef instruction)
    {
        if (instruction.IsAReturnInst != default)
        {
            return FlowControl.Return;
        }
        else if (instruction.IsAUnreachableInst != default)
        {
            return FlowControl.Unreachable;
        }
        else if (instruction.IsAIndirectBrInst != default)
        {
            return FlowControl.IndirectBranch;
        }
        else if (instruction.IsABranchInst != default)
        {
            return instruction.IsConditional
                ? FlowControl.ConditionalBranch
                : FlowControl.UnconditionalBranch;
        }
        else if (instruction.IsASwitchInst != default)
        {
            return FlowControl.ConditionalBranch; // Todo: check if this is redundant
        }
        else
        {
            return FlowControl.Normal;
        }
    }
}
