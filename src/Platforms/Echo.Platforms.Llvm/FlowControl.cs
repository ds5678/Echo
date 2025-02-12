namespace Echo.Platforms.Llvm;

internal enum FlowControl
{
    Normal,
    UnconditionalBranch,
    ConditionalBranch,
    IndirectBranch,
    Return,
    Unreachable,
}
