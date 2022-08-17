using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DisableDateTimeNow;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisableDateTimeNowAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "DDTN0001";
	private const string Category = "Illegal Method Calls";
	private static readonly string Description = "DateTimeAnalyzerDescription";
	private static readonly string MessageFormat = "DateTimeAnalyzerMessageFormat";
	private static readonly string Title = "DateTimeAnalyzerMessageFormat";

	private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
		DiagnosticSeverity.Error, true, Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
		                                       GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.RegisterCompilationStartAction(compilationStartContext =>
		{
			var dateTimeType = compilationStartContext.Compilation.GetTypeByMetadataName("System.DateTime");
			compilationStartContext.RegisterSyntaxNodeAction(analysisContext =>
			{
				var invocations = analysisContext.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
				foreach (var invocation in invocations)
				{
					ExpressionSyntax e = invocation.Expression switch
					{
						MemberAccessExpressionSyntax mex => mex,
						IdentifierNameSyntax ins => ins,
						_ => null
					};

					if (e == null)
					{
						continue;
					}

					var typeInfo = analysisContext.SemanticModel.GetTypeInfo(e).Type as INamedTypeSymbol;
					if (typeInfo?.ConstructedFrom == null)
					{
						continue;
					}

					if (!typeInfo.ConstructedFrom.Equals(dateTimeType, SymbolEqualityComparer.Default))
					{
						continue;
					}

					if (invocation.Name.ToString() == "Now")
					{
						analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
					}
				}
			}, SyntaxKind.None);
		});
	}
}