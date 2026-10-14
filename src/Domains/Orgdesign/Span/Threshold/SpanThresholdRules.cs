namespace Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>A rule a span threshold must pass. Check returns its problems, or none.</summary>
public interface ISpanThresholdRule
{
    List<string> Check(SpanThreshold item);
}

public sealed record SpanThresholdCodePrefixRule(string Prefix) : ISpanThresholdRule
{
    public List<string> Check(SpanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanThresholdCodeLengthRule(int MaxLength) : ISpanThresholdRule
{
    public List<string> Check(SpanThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanThresholdChecksumLengthRule(int MaxLength) : ISpanThresholdRule
{
    public List<string> Check(SpanThreshold item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span threshold.</summary>
public sealed record SpanThresholdValidator(List<ISpanThresholdRule> Rules)
{
    public List<string> Validate(SpanThreshold item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(SpanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanThreshold> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of span threshold rules.</summary>
public static class SpanThresholdRules
{
    public static SpanThresholdValidator DefaultSpanThresholdValidator()
    {
        return new SpanThresholdValidator(
            new List<ISpanThresholdRule>
            {
                new SpanThresholdCodePrefixRule("Q"),
                new SpanThresholdCodeLengthRule(255),
                new SpanThresholdChecksumLengthRule(120),
            });
    }
}
