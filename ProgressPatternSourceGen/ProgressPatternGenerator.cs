using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProgressPatternSourceGen;

[Generator]
public class ProgressPatternGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => Path.GetFileName(file.Path) == "progressPatterns.json");

        var patternsProvider = jsonFiles.Select((file, cancellationToken) =>
        {
            var text = file.GetText(cancellationToken)?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return null;
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, ProgressPatternJson>>(text);
            }
            catch
            {
                return null;
            }
        });

        context.RegisterSourceOutput(patternsProvider, (spc, patterns) =>
        {
            if (patterns == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("namespace progress_bar_dotnet {");
            sb.AppendLine("public abstract partial class ProgressPattern {");

            foreach (var kvp in patterns)
            {
                var name = ToPascalCase(kvp.Key) + "ProgressPattern";
                sb.AppendLine($"    private sealed class {name} : ProgressPattern {{");
                var charLiterals = string.Join(", ", kvp.Value.pattern.Select(p => ToCharLiteral(p)));
                sb.AppendLine($"        public override IReadOnlyList<char> Pattern => new[] {{ {charLiterals} }};");
                sb.AppendLine("    }");
            }

            sb.AppendLine("    public static class Known {");
            foreach (var kvp in patterns)
            {
                var name = ToPascalCase(kvp.Key) + "ProgressPattern";
                sb.AppendLine($"        public static ProgressPattern {ToPascalCase(kvp.Key)} {{ get; }} = new {name}();");
            }
            var defaultPattern = patterns.FirstOrDefault(p => p.Value.isDefault).Key;
            if (!string.IsNullOrEmpty(defaultPattern))
                sb.AppendLine($"        public static ProgressPattern Default {{ get; }} = new {ToPascalCase(defaultPattern)}ProgressPattern();");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("}");

            spc.AddSource("ProgressPattern.Generated.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

            // Emit a test file to confirm generator is running
            spc.AddSource("__GeneratorTest.cs", SourceText.From("// Generator is running!", Encoding.UTF8));
        });
    }

    private static string ToPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    private static string ToCharLiteral(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length != 1)
            throw new ArgumentException($"Pattern entry '{s}' is not a single character.");
        var c = s[0];
        switch (c)
        {
            case '\\': return "'\\\\'";
            case '\'': return "'\\\''";
            case '"': return "'\\\"'";
            default:
                if (char.IsControl(c) || char.IsWhiteSpace(c) || c > 127)
                    return $"'\\u{(int)c:x4}'";
                return $"'{c}'";
        }
    }

    private class ProgressPatternJson
    {
        public bool isDefault { get; set; }
        public List<string> pattern { get; set; }
    }
}
