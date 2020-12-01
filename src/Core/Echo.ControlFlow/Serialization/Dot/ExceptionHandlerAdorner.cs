using System;
using System.Collections.Generic;
using Echo.ControlFlow.Regions;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds styles to regions in control flow graphs. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ExceptionHandlerAdorner<TInstruction> : IDotSubGraphAdorner
    {
        /// <summary>
        /// Gets or sets the style of an exception handler region.
        /// </summary>
        public DotEntityStyle ExceptionHandlerStyle
        {
            get;
            set;
        } = new DotEntityStyle("red", "solid");

        /// <summary>
        /// Gets or sets the style of the protected region in an exception handler region.
        /// </summary>
        public DotEntityStyle ProtectedRegionColor
        {
            get;
            set;
        } = new DotEntityStyle("green", "dashed");

        /// <summary>
        /// Gets or sets the style of a prologue region in an exception handler region.
        /// </summary>
        public DotEntityStyle PrologueRegionColor
        {
            get;
            set;
        } = new DotEntityStyle("purple", "dashed");

        /// <summary>
        /// Gets or sets the style of a handler region in an exception handler region.
        /// </summary>
        public DotEntityStyle HandlerRegionStyle
        {
            get;
            set;
        } = new DotEntityStyle("blue", "dashed");

        /// <summary>
        /// Gets or sets the style of an epilogue region in an exception handler region.
        /// </summary>
        public DotEntityStyle EpilogueRegionColor
        {
            get;
            set;
        } = new DotEntityStyle("orange", "dashed");

        /// <summary>
        /// Gets or sets the default style of a control flow region.
        /// </summary>
        public DotEntityStyle DefaultRegionColor
        {
            get;
            set;
        } = new DotEntityStyle("gray", "dashed");
        
        /// <inheritdoc />
        public string GetSubGraphName(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return null;
            
            string prefix = DetermineRegionPrefix(region);

            long min = long.MaxValue;
            long max = long.MinValue;
            foreach (var node in region.GetNodes())
            {
                min = Math.Min(min, node.Offset);
                max = Math.Max(max, node.Offset);
            }

            return $"{prefix}_{min:X}_{max:X}";
        }

        private static string DetermineRegionPrefix(IControlFlowRegion<TInstruction> region)
        {
            string prefix = null;
            switch (region)
            {
                case ScopeRegion<TInstruction> basicRegion:
                    if (basicRegion.ParentRegion is ExceptionHandlerRegion<TInstruction> parentEh)
                    {
                        if (parentEh.ProtectedRegion == basicRegion)
                            prefix = "cluster_protected";
                    }

                    prefix ??= "cluster_block";
                    break;

                case ExceptionHandlerRegion<TInstruction> _:
                    prefix = "cluster_eh";
                    break;

                case HandlerRegion<TInstruction> _:
                    prefix = "cluster_handler";
                    break;
                
                default:
                    prefix = "cluster_region";
                    break;
            }

            return prefix;
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetSubGraphAttributes(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return null;
            
            var regionStyle = DefaultRegionColor;
            switch (region)
            {
                case ScopeRegion<TInstruction> basicRegion:
                    if (basicRegion.ParentRegion is ExceptionHandlerRegion<TInstruction> parentEh)
                    {
                        if (parentEh.ProtectedRegion == basicRegion)
                            regionStyle = ProtectedRegionColor;
                    }

                    break;

                case ExceptionHandlerRegion<TInstruction> _:
                    regionStyle = ExceptionHandlerStyle;
                    break;

                case HandlerRegion<TInstruction> _:
                    regionStyle = HandlerRegionStyle;
                    break;
            }
            
            return new Dictionary<string, string>
            {
                ["color"] = regionStyle.Color,
                ["style"] = regionStyle.Style
            };
        }

    }
}