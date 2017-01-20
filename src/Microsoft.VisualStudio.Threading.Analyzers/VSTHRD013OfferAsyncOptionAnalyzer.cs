﻿namespace Microsoft.VisualStudio.Threading.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CodeAnalysis;
    using CodeAnalysis.CSharp;
    using CodeAnalysis.CSharp.Syntax;
    using CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VSTHRD013OfferAsyncOptionAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "VSTHRD013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: Id,
            title: Strings.VSTHRD013_Title,
            messageFormat: Strings.VSTHRD013_MessageFormat,
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            context.RegisterCodeBlockStartAction<SyntaxKind>(ctxt =>
            {
                // We want to scan invocations that occur inside internal, synchronous methods
                // for calls to JTF.Run or JT.Join.
                var methodSymbol = ctxt.OwningSymbol as IMethodSymbol;
                if (!methodSymbol.HasAsyncCompatibleReturnType() && Utils.IsPublic(methodSymbol) && !Utils.IsEntrypointMethod(methodSymbol, ctxt.SemanticModel, ctxt.CancellationToken) && !methodSymbol.HasAsyncAlternative(ctxt.CancellationToken))
                {
                    var methodAnalyzer = new MethodAnalyzer();
                    ctxt.RegisterSyntaxNodeAction(methodAnalyzer.AnalyzeInvocation, SyntaxKind.InvocationExpression);
                    ctxt.RegisterSyntaxNodeAction(methodAnalyzer.AnalyzePropertyGetter, SyntaxKind.SimpleMemberAccessExpression);
                }
            });
        }

        private class MethodAnalyzer
        {
            private bool diagnosticReported;

            internal void AnalyzePropertyGetter(SyntaxNodeAnalysisContext context)
            {
                var memberAccessSyntax = (MemberAccessExpressionSyntax)context.Node;
                this.InspectMemberAccess(context, memberAccessSyntax, CommonInterest.SyncBlockingProperties);
            }

            internal void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
            {
                var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
                this.InspectMemberAccess(context, invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax, CommonInterest.JTFSyncBlockers);
            }

            private void InspectMemberAccess(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccessSyntax, IReadOnlyList<CommonInterest.SyncBlockingMethod> problematicMethods)
            {
                if (memberAccessSyntax == null)
                {
                    return;
                }

                if (this.diagnosticReported)
                {
                    // Don't report more than once per method.
                    return;
                }

                if (context.Node.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() != null)
                {
                    // We do not analyze JTF.Run inside anonymous functions because
                    // they are so often used as callbacks where the signature is constrained.
                    return;
                }

                var typeReceiver = context.SemanticModel.GetTypeInfo(memberAccessSyntax.Expression).Type;
                if (typeReceiver != null)
                {
                    foreach (var item in problematicMethods)
                    {
                        if (memberAccessSyntax.Name.Identifier.Text == item.MethodName &&
                            typeReceiver.Name == item.ContainingTypeName &&
                            typeReceiver.BelongsToNamespace(item.ContainingTypeNamespace))
                        {
                            var location = memberAccessSyntax.Name.GetLocation();
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                            this.diagnosticReported = true;
                        }
                    }
                }
            }
        }
    }
}
