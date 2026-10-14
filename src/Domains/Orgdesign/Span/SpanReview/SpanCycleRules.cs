namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>A rule a span cycle must pass. Check returns its problems, or none.</summary>
public interface ISpanCycleRule
{
    List<string> Check(SpanCycle item);
}

public sealed record SpanCycleTitleLengthRule(int MaxLength) : ISpanCycleRule
{
    public List<string> Check(SpanCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanCycleHoursRangeRule(double Min, double Max) : ISpanCycleRule
{
    public List<string> Check(SpanCycle item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanCycleCodePrefixRule(string Prefix) : ISpanCycleRule
{
    public List<string> Check(SpanCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span cycle.</summary>
public sealed record SpanCycleValidator(List<ISpanCycleRule> Rules)
{
    public List<string> Validate(SpanCycle item)
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

    public void RequireValid(SpanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanCycle> items)
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

/// <summary>Functions of span cycle rules.</summary>
public static class SpanCycleRules
{
    public static SpanCycleValidator DefaultSpanCycleValidator()
    {
        return new SpanCycleValidator(
            new List<ISpanCycleRule>
            {
                new SpanCycleTitleLengthRule(64),
                new SpanCycleHoursRangeRule(1.7, 1810.3),
                new SpanCycleCodePrefixRule("X"),
            });
    }
}
