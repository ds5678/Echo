using Disarm;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Platforms.Disarm;

/// <summary>
/// Provides a custom formatter for <see cref="Arm64Instruction"/>s.
/// </summary>
public class Arm64InstructionFormatter : IInstructionFormatter<Arm64Instruction>
{
    /// <inheritdoc />
    public string Format(in Arm64Instruction instruction) => $"{instruction}";
}