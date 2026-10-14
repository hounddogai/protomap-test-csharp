namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>A rule a span estimate must pass. Check returns its problems, or none.</summary>
public interface ISpanEstimateRule
{
    List<string> Check(SpanEstimate item);
}

public sealed record SpanEstimateNameLengthRule(int MaxLength) : ISpanEstimateRule
{
    public List<string> Check(SpanEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanEstimateWindowDaysRangeRule(int Min, int Max) : ISpanEstimateRule
{
    public List<string> Check(SpanEstimate item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanEstimateCodeLengthRule(int MaxLength) : ISpanEstimateRule
{
    public List<string> Check(SpanEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanEstimateUpdatedAtRangeRule(int Min, int Max) : ISpanEstimateRule
{
    public List<string> Check(SpanEstimate item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a span estimate.</summary>
public sealed record SpanEstimateValidator(List<ISpanEstimateRule> Rules)
{
    public List<string> Validate(SpanEstimate item)
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

    public void RequireValid(SpanEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanEstimate> items)
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

/// <summary>Functions of span estimate rules.</summary>
public static class SpanEstimateRules
{
    public static SpanEstimateValidator DefaultSpanEstimateValidator()
    {
        return new SpanEstimateValidator(
            new List<ISpanEstimateRule>
            {
                new SpanEstimateNameLengthRule(1000),
                new SpanEstimateWindowDaysRangeRule(16, 80),
                new SpanEstimateCodeLengthRule(64),
                new SpanEstimateUpdatedAtRangeRule(32, 1000),
            });
    }
}
