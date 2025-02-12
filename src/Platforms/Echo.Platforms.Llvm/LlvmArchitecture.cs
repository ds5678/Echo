using Echo.Code;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;

namespace Echo.Platforms.Llvm;

/// <summary>
/// Represents an architecture description for LLVM IR.
/// </summary>
public class LlvmArchitecture : IArchitecture<LLVMValueRef>, IStaticSuccessorResolver<LLVMValueRef>, IStaticInstructionProvider<LLVMValueRef>
{
    private readonly List<LLVMValueRef> _instructionsList = new();
    private readonly Dictionary<LLVMValueRef, (int Offset, LlvmInstructionResult Variable)> _instructionsDictionary = new();
    private readonly Dictionary<LLVMValueRef, LlvmParameter> _parameters = new();

    IArchitecture<LLVMValueRef> IStaticInstructionProvider<LLVMValueRef>.Architecture => this;

    /// <summary>
    /// Creates a new LLVM architecture description based on a function.
    /// </summary>
    /// <param name="function">The function.</param>
    public LlvmArchitecture(LLVMValueRef function)
    {
        if (function.IsAFunction == default)
        {
            throw new ArgumentException("Provided value is not a function.", nameof(function));
        }

        foreach (var parameter in function.Params)
        {
            int index = _parameters.Count;
            var llvmParameter = new LlvmParameter(parameter, index);
            _parameters.Add(parameter, llvmParameter);
        }

        foreach (var block in function.BasicBlocks)
        {
            foreach (var instruction in block.GetInstructions())
            {
                _instructionsDictionary.Add(instruction, (_instructionsDictionary.Count, new LlvmInstructionResult(instruction)));
                _instructionsList.Add(instruction);
            }
        }
    }

    /// <inheritdoc />
    public InstructionFlowControl GetFlowControl(in LLVMValueRef instruction)
    {
        return instruction.GetFlowControl() switch
        {
            FlowControl.Normal => InstructionFlowControl.Fallthrough,
            FlowControl.UnconditionalBranch => InstructionFlowControl.CanBranch,
            FlowControl.ConditionalBranch => InstructionFlowControl.CanBranch,
            FlowControl.IndirectBranch => InstructionFlowControl.CanBranch,
            FlowControl.Return => InstructionFlowControl.IsTerminator,
            FlowControl.Unreachable => InstructionFlowControl.IsTerminator,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <inheritdoc />
    public void GetReadVariables(in LLVMValueRef instruction, ICollection<IVariable> variablesBuffer)
    {
        foreach (var operand in instruction.GetOperands())
        {
            if (operand.IsAInstruction != default)
            {
                variablesBuffer.Add(_instructionsDictionary[operand].Variable);
            }
            else if (operand.IsAArgument != default)
            {
                variablesBuffer.Add(_parameters[operand]);
            }
        }
    }

    /// <inheritdoc />
    public void GetWrittenVariables(in LLVMValueRef instruction, ICollection<IVariable> variablesBuffer)
    {
        if (instruction.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
        {
            variablesBuffer.Add(_instructionsDictionary[instruction].Variable);
        }
    }

    /// <inheritdoc />
    public void GetSuccessors(in LLVMValueRef instruction, IList<SuccessorInfo> successorsBuffer)
    {
        switch (instruction.GetFlowControl())
        {
            case FlowControl.UnconditionalBranch:
            case FlowControl.ConditionalBranch:
                AddBranchSuccessors(instruction, successorsBuffer);
                break;

            case FlowControl.IndirectBranch:
            case FlowControl.Return:
            case FlowControl.Unreachable:
                break;

            default:
                FallThrough(instruction, successorsBuffer);
                break;
        }

        void AddBranchSuccessors(LLVMValueRef instruction, IList<SuccessorInfo> successorsBuffer)
        {
            foreach (var operand in instruction.GetOperands())
            {
                if (operand.IsBasicBlock)
                {
                    successorsBuffer.Add(new SuccessorInfo(
                        _instructionsDictionary[operand.AsBasicBlock().FirstInstruction].Offset,
                        ControlFlowEdgeType.Conditional
                    ));
                }
            }
        }

        void FallThrough(LLVMValueRef instruction, IList<SuccessorInfo> successorsBuffer)
        {
            var fallthrough = instruction.NextInstruction;
            if (fallthrough != default)
            {
                successorsBuffer.Add(new SuccessorInfo(
                    _instructionsDictionary[fallthrough].Offset,
                    ControlFlowEdgeType.FallThrough
                ));
            }
        }
    }

    long IArchitecture<LLVMValueRef>.GetOffset(in LLVMValueRef instruction)
    {
        return _instructionsDictionary[instruction].Offset;
    }

    int IArchitecture<LLVMValueRef>.GetSize(in LLVMValueRef instruction)
    {
        return 1;
    }

    int IArchitecture<LLVMValueRef>.GetStackPopCount(in LLVMValueRef instruction)
    {
        return 0; // Instructions don't pop from a stack.
    }

    int IArchitecture<LLVMValueRef>.GetStackPushCount(in LLVMValueRef instruction)
    {
        return 0; // Instructions don't push to a stack.
    }

    LLVMValueRef IStaticInstructionProvider<LLVMValueRef>.GetInstructionAtOffset(long offset)
    {
        return _instructionsList[(int)offset];
    }
}
