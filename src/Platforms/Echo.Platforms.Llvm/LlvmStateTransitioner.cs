using Echo.Code;
using Echo.ControlFlow;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using LLVMSharp.Interop;
using System.Collections.Generic;

namespace Echo.Platforms.Llvm;

/// <summary>
/// Provides an implementation of the <see cref="IStateTransitioner{TInstruction}"/> interface, that
/// implements the state transitioning for the LLVM IR instruction set.
/// </summary>
public class LlvmStateTransitioner : StateTransitioner<LLVMValueRef>
{
    /// <summary>
    /// Creates a new instance of <see cref="LlvmStateTransitioner"/>.
    /// </summary>
    /// <param name="architecture">The LLVM architecture instance.</param>
    public LlvmStateTransitioner(IArchitecture<LLVMValueRef> architecture)
        : base(architecture)
    {
    }

    /// <inheritdoc />
    public override void GetTransitions(
        in SymbolicProgramState<LLVMValueRef> currentState,
        in LLVMValueRef instruction,
        IList<StateTransition<LLVMValueRef>> transitionsBuffer)
    {
        var nextState = ApplyDefaultBehaviour(currentState, instruction);

        switch (instruction.GetFlowControl())
        {
            case FlowControl.UnconditionalBranch:
                UnconditionalBranch(instruction, nextState, transitionsBuffer);
                break;

            case FlowControl.ConditionalBranch:
                ConditionalBranch(instruction, nextState, transitionsBuffer);
                break;

            case FlowControl.IndirectBranch:
                //TODO: Try inferring indirect branch from data flow graph.
                break;

            case FlowControl.Return:
            case FlowControl.Unreachable:
                break;

            default:
                FallThrough(nextState, transitionsBuffer);
                break;
        }
    }

    private void UnconditionalBranch(
        in LLVMValueRef instruction,
        in SymbolicProgramState<LLVMValueRef> nextState,
        IList<StateTransition<LLVMValueRef>> successorBuffer)
    {
        var branchState = nextState.WithProgramCounter(Architecture.GetOffset(instruction.GetOperand(0)));
        successorBuffer.Add(new StateTransition<LLVMValueRef>(branchState, ControlFlowEdgeType.Unconditional));
    }

    private void ConditionalBranch(
        in LLVMValueRef instruction,
        in SymbolicProgramState<LLVMValueRef> nextState,
        IList<StateTransition<LLVMValueRef>> successorBuffer)
    {
        var branchState1 = nextState.WithProgramCounter(Architecture.GetOffset(instruction.GetOperand(1)));
        var branchState2 = nextState.WithProgramCounter(Architecture.GetOffset(instruction.GetOperand(2)));
        successorBuffer.Add(new StateTransition<LLVMValueRef>(branchState1, ControlFlowEdgeType.Conditional));
        successorBuffer.Add(new StateTransition<LLVMValueRef>(branchState2, ControlFlowEdgeType.Conditional));
    }

    private static void FallThrough(
        in SymbolicProgramState<LLVMValueRef> nextState,
        IList<StateTransition<LLVMValueRef>> successorBuffer)
    {
        successorBuffer.Add(new StateTransition<LLVMValueRef>(nextState, ControlFlowEdgeType.FallThrough));
    }
}
