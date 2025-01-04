using Disarm.InternalDisassembly;
using Echo.Code;

namespace Echo.Platforms.Disarm;

/// <summary>
/// Represents a variable represented by an arm64 general purpose register. 
/// </summary>
public class Arm64GeneralRegister : IVariable
{
    /// <summary>
    /// Creates a new arm64 register variable.
    /// </summary>
    /// <param name="register">The register.</param>
    public Arm64GeneralRegister(Arm64Register register)
    {
        Register = register;
    }

    /// <inheritdoc />
    public string Name => Register.ToString();

    /// <summary>
    /// Gets the register this variable is referencing.
    /// </summary>
    public Arm64Register Register
    {
        get;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}