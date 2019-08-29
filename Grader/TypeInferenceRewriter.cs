﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Grader
{
    public class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;

        public TypeInferenceRewriter(SemanticModel model)
        {
            _model = model;
        }



        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node.Expression is InvocationExpressionSyntax invoke)
            {
                var symbol = _model.GetSymbolInfo(invoke);

                var result = symbol.Symbol.ToString();
                if (result.StartsWith("System.Console"))
                {
                    if (invoke.Expression is IdentifierNameSyntax methodId)
                    {
                        var grader = SyntaxFactory.IdentifierName("Grader");
                        var console = SyntaxFactory.IdentifierName("Console");

                        var consoleSpace = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            grader, console);
                        var nameSpace = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, consoleSpace, methodId);
                        var newNode = node.ReplaceNode(invoke.Expression, nameSpace);
                        return newNode;
                    }
                    if (invoke.Expression is MemberAccessExpressionSyntax memberAccess)
                    {

                        if (memberAccess.Expression is IdentifierNameSyntax id)
                        {
                            if (id.Identifier.Text == "Console")
                            {
                                var grader = SyntaxFactory.IdentifierName("Grader");
                                var nameSpace = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, grader, id);
                                var newNode = node.ReplaceNode(memberAccess.Expression, nameSpace);
                                return newNode;
                            }
                        }
                        else

                        if (memberAccess.Expression is MemberAccessExpressionSyntax systemAccess)
                        {
                            if (systemAccess.Expression is IdentifierNameSyntax systemId)
                            {
                                if (systemId.Identifier.Text == "System")
                                {
                                    var newNode = node.ReplaceNode(systemId, SyntaxFactory.IdentifierName("Grader"));
                                    return newNode;
                                }
                            }
                        }
                    }
                }

            }
            return base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count > 1)
            {
                return node;
            }
            if (node.Declaration.Variables[0].Initializer == null)
            {
                return node;
            }

            VariableDeclaratorSyntax declarator = node.Declaration.Variables.First();
            TypeSyntax variableTypeName = node.Declaration.Type;

            ITypeSymbol variableType =
                (ITypeSymbol)_model.GetSymbolInfo(variableTypeName)
                    .Symbol;

            TypeInfo initializerInfo =
                _model.GetTypeInfo(declarator
                    .Initializer
                    .Value);

            if (variableType == initializerInfo.Type)
            {
                TypeSyntax varTypeName =
                    SyntaxFactory.IdentifierName("var")
                        .WithLeadingTrivia(
                            variableTypeName.GetLeadingTrivia())
                        .WithTrailingTrivia(
                            variableTypeName.GetTrailingTrivia());

                return node.ReplaceNode(variableTypeName, varTypeName);
            }
            else
            {
                return node;
            }
        }
    }
}