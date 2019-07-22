using System.IO;
using Echo.ControlFlow.Specialized;
using Echo.Core.Code;

namespace Echo.ControlFlow.Serialization.Dot
{
    public class BasicBlockDotWriter<TInstruction> : DotWriter
        where TInstruction : IInstruction
    {
        public BasicBlockDotWriter(TextWriter writer) 
            : base(writer)
        {
        }

        protected override void Write(INode node, string identifier)
        {
            WriteIdentifier(identifier);
            
            Writer.Write(" [shape=box3d, label=");
            string code = string.Join("\\l", ((Node<BasicBlock<TInstruction>>) node).Contents.Instructions) + "\\l";
            WriteIdentifier(code);
            Writer.Write(']');
            WriteSemicolon();
            Writer.WriteLine();
        }
        
    }
}