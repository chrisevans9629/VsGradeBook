﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Grader
{
    public class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;

        public TypeInferenceRewriter(SemanticModel model)
        {
            _model = model;
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            if(node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (node.Expression is InvocationExpressionSyntax syntax)
            {
                if (InvocationExpressionStatementSyntax(node, syntax, out var newNode))
                {
                    return newNode;
                }
            }
            return base.VisitArgument(node);
        }

       
        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (node.Expression is InvocationExpressionSyntax invoke)
            {
                if (InvocationExpressionStatementSyntax(node, invoke, out var syntaxNode))
                {
                    return syntaxNode;
                }
            }
            return base.VisitExpressionStatement(node);
        }
        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            foreach (var variableDeclaratorSyntax in node.Declaration.Variables)
            {
                var value = variableDeclaratorSyntax.Initializer.Value;
                if (value is InvocationExpressionSyntax invoke)
                {
                    if (InvocationExpressionStatementSyntax(node, invoke, out var syntaxNode))
                    {
                        return syntaxNode;
                    }
                }
            }
            return base.VisitLocalDeclarationStatement(node);

        }


        public bool HasChanges { get; set; }
        private bool InvocationExpressionStatementSyntax(SyntaxNode node, InvocationExpressionSyntax invoke,
            out SyntaxNode syntaxNode)
        {
            syntaxNode = null;
            var symbol = _model.GetSymbolInfo(invoke);
            
            var result = symbol.Symbol?.ToString();
            if (result?.StartsWith("System.Console", StringComparison.InvariantCulture) == true)
            {
                if (invoke.Expression is IdentifierNameSyntax methodId)
                {
                    var grader = SyntaxFactory.IdentifierName("Grader");
                    var console = SyntaxFactory.IdentifierName("Console");

                    var consoleSpace = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        grader, console);
                    var nameSpace =
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, consoleSpace, methodId);
                    var newNode = node.ReplaceNode(invoke.Expression, nameSpace);
                    syntaxNode = newNode;
                    HasChanges = true;
                    return HasChanges;
                }

                if (invoke.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is IdentifierNameSyntax id)
                    {
                        if (id.Identifier.Text == "Console")
                        {
                            var grader = SyntaxFactory.IdentifierName("Grader");
                            var nameSpace =
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, grader, id.WithoutTrivia()).NormalizeWhitespace();
                            var newNode = node.ReplaceNode(memberAccess.Expression, nameSpace);
                            syntaxNode = newNode;
                            HasChanges = true;
                            return HasChanges;
                        }
                    }
                    else if (memberAccess.Expression is MemberAccessExpressionSyntax systemAccess)
                    {
                        if (systemAccess.Expression is IdentifierNameSyntax systemId)
                        {
                            if (systemId.Identifier.Text == "System")
                            {
                                var newNode = node.ReplaceNode(systemId, SyntaxFactory.IdentifierName("Grader"));
                                syntaxNode = newNode;
                                HasChanges = true;
                                return HasChanges;
                            }
                        }
                    }
                }
            }

            return false;
        }

      
    }
}