using Disarm;
using Disarm.InternalDisassembly;
using Echo.Code;
using System;
using System.Collections.Generic;

namespace Echo.Platforms.Disarm;

/// <summary>
/// Provides a description of the arm64 instruction set architecture (ISA) that is modeled by Disarm.
/// </summary>
public class Arm64Architecture : IArchitecture<Arm64Instruction>
{
    private readonly IDictionary<Arm64Register, Arm64GeneralRegister> _gpr = new Dictionary<Arm64Register, Arm64GeneralRegister>();

    /// <summary>
    /// Creates a new instance of the <see cref="Arm64Architecture"/> class.
    /// </summary>
    public Arm64Architecture()
    {
        foreach (Arm64Register register in Enum.GetValues(typeof(Arm64Register)))
            _gpr[register] = new Arm64GeneralRegister(register);
    }

    /// <summary>
    /// Gets a register variable by its identifier.
    /// </summary>
    /// <param name="register">The register identifier.</param>
    /// <returns>The register variable.</returns>
    public Arm64GeneralRegister GetRegister(Arm64Register register) => _gpr[register];

    /// <inheritdoc />
    public InstructionFlowControl GetFlowControl(in Arm64Instruction instruction)
    {
        return instruction.MnemonicCategory switch
        {
            Arm64MnemonicCategory.Branch => InstructionFlowControl.CanBranch,
            Arm64MnemonicCategory.ConditionalBranch => InstructionFlowControl.CanBranch | InstructionFlowControl.Fallthrough,
            Arm64MnemonicCategory.Return => InstructionFlowControl.IsTerminator,
            _ => InstructionFlowControl.Fallthrough,
        };
    }

    /// <inheritdoc />
    public long GetOffset(in Arm64Instruction instruction)
    {
        return (long)instruction.Address;
    }

    /// <inheritdoc />
    public int GetStackPopCount(in Arm64Instruction instruction)
    {
        // TODO:
        return 0;
    }

    /// <inheritdoc />
    public int GetStackPushCount(in Arm64Instruction instruction)
    {
        // TODO:
        return 0;
    }

    /// <inheritdoc />
    public void GetReadVariables(in Arm64Instruction instruction, ICollection<IVariable> variablesBuffer)
    {
        // TODO:
    }

    /// <inheritdoc />
    public void GetWrittenVariables(in Arm64Instruction instruction, ICollection<IVariable> variablesBuffer)
    {
        // TODO:
    }

    int IArchitecture<Arm64Instruction>.GetSize(in Arm64Instruction instruction)
    {
        return sizeof(uint); // All ARM64 instructions are 4 bytes long.
    }
}
