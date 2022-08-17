using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace DisableDateTimeNow;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisableDateTimeNowCodeFixProvider))]
[Shared]
public class DisableDateTimeNowCodeFixProvider : CodeFixProvider
{
    private static readonly string Title = "DisableDateTimeNowCodeFixProviderTitle";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DisableDateTimeNowAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
                Title),
            diagnostic);

        return Task.CompletedTask;
    }

    private async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span,
        CancellationToken ct)
    {
        var text = await document.GetTextAsync(ct);
        var repl = "DateTime.UtcNow";
        if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
        {
            repl = "System.DateTime.UtcNow";
        }

        var newText = text.Replace(span, repl);
        return document.WithText(newText);
    }
}