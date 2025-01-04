using Disarm;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using System.Collections.Generic;

namespace Echo.Platforms.Disarm;

/// <summary>
/// Provides an implementation for the <see cref="IStaticSuccessorResolver{TInstruction}"/> that is able to
/// obtain successor information of arm64 instructions modeled by the <see cref="Arm64Instruction"/> structure.
/// </summary>
public class Arm64StaticSuccessorResolver : IStaticSuccessorResolver<Arm64Instruction>
{
    /// <inheritdoc />
    public void GetSuccessors(in Arm64Instruction instruction, IList<SuccessorInfo> successorsBuffer)
    {
        switch (instruction.MnemonicCategory)
        {
            case Arm64MnemonicCategory.Branch:
                UnconditionalBranch(instruction, successorsBuffer);
                break;
            case Arm64MnemonicCategory.ConditionalBranch:
                ConditionalBranch(instruction, successorsBuffer);
                break;
            case Arm64MnemonicCategory.Return:
                break;
            default:
                FallThrough(instruction, successorsBuffer);
                break;
        }
    }

    private static void UnconditionalBranch(in Arm64Instruction instruction, IList<SuccessorInfo> successorsBuffer)
    {
        successorsBuffer.Add(new SuccessorInfo(
            (long)instruction.BranchTarget,
            ControlFlowEdgeType.Unconditional
        ));
    }

    private static void ConditionalBranch(in Arm64Instruction instruction, IList<SuccessorInfo> successorsBuffer)
    {
        successorsBuffer.Add(new SuccessorInfo(
            (long)instruction.BranchTarget,
            ControlFlowEdgeType.Conditional
        ));
        successorsBuffer.Add(new SuccessorInfo(
            (long)instruction.Address + sizeof(uint),
            ControlFlowEdgeType.FallThrough
        ));
    }

    private static void FallThrough(in Arm64Instruction instruction, IList<SuccessorInfo> successorsBuffer)
    {
        successorsBuffer.Add(new SuccessorInfo(
            (long)instruction.Address + sizeof(uint),
            ControlFlowEdgeType.FallThrough
        ));
    }
}
