﻿using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast
{
    internal sealed class UniqueVariableFinderVisitor<TInstruction> : IAstNodeVisitor<TInstruction, object>
    {
        private readonly HashSet<IVariable> _variables = new HashSet<IVariable>();

        internal int Count => _variables.Count;

        internal IVariable[] Variables => _variables.ToArray();

        public void Visit(AstAssignmentStatement<TInstruction> assignmentStatement, object state)
        {
            foreach (AstNodeBase<TInstruction> child in assignmentStatement.GetChildren())
                child.Accept(this, state);
        }

        public void Visit(AstExpressionStatement<TInstruction> expressionStatement, object state) =>
            expressionStatement.Expression.Accept(this, state);

        public void Visit(AstInstructionExpression<TInstruction> instructionExpression, object state) { }

        public void Visit(AstVariableExpression<TInstruction> variableExpression, object state) =>
            _variables.Add(variableExpression.Variable);
    }
}