using Disarm;
using Echo.Code;
using Echo.ControlFlow;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using System.Collections.Generic;

namespace Echo.Platforms.Disarm;

/// <summary>
/// Provides an implementation of the <see cref="IStateTransitioner{TInstruction}"/> interface, that
/// implements the state transitioning for the arm64 instruction set.
/// </summary>
public class Arm64StateTransitioner : StateTransitioner<Arm64Instruction>
{
    /// <summary>
    /// Creates a new instance of <see cref="Arm64StateTransitioner"/>.
    /// </summary>
    /// <param name="architecture">The arm64 architecture instance.</param>
    public Arm64StateTransitioner(IArchitecture<Arm64Instruction> architecture)
        : base(architecture)
    {
    }

    /// <inheritdoc />
    public override void GetTransitions(
        in SymbolicProgramState<Arm64Instruction> currentState,
        in Arm64Instruction instruction,
        IList<StateTransition<Arm64Instruction>> transitionsBuffer)
    {
        var nextState = ApplyDefaultBehaviour(currentState, instruction);

        switch (instruction.MnemonicCategory)
        {
            case Arm64MnemonicCategory.Branch:
                UnconditionalBranch(instruction, nextState, transitionsBuffer);
                break;

            case Arm64MnemonicCategory.ConditionalBranch:
                ConditionalBranch(instruction, nextState, transitionsBuffer);
                break;

            case Arm64MnemonicCategory.Return:
                break;

            default:
                FallThrough(nextState, transitionsBuffer);
                break;
        }
    }

    private static void UnconditionalBranch(
        in Arm64Instruction instruction,
        in SymbolicProgramState<Arm64Instruction> nextState,
        IList<StateTransition<Arm64Instruction>> successorBuffer)
    {
        var branchState = nextState.WithProgramCounter((long)instruction.BranchTarget);
        successorBuffer.Add(new StateTransition<Arm64Instruction>(branchState, ControlFlowEdgeType.Unconditional));
    }

    private static void ConditionalBranch(
        in Arm64Instruction instruction,
        in SymbolicProgramState<Arm64Instruction> nextState,
        IList<StateTransition<Arm64Instruction>> successorBuffer)
    {
        var branchState = nextState.WithProgramCounter((long)instruction.BranchTarget);
        successorBuffer.Add(new StateTransition<Arm64Instruction>(branchState, ControlFlowEdgeType.Conditional));
        successorBuffer.Add(new StateTransition<Arm64Instruction>(nextState, ControlFlowEdgeType.FallThrough));
    }

    private static void FallThrough(
        in SymbolicProgramState<Arm64Instruction> nextState,
        IList<StateTransition<Arm64Instruction>> successorBuffer)
    {
        successorBuffer.Add(new StateTransition<Arm64Instruction>(nextState, ControlFlowEdgeType.FallThrough));
    }
}