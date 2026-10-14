namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>A rule a span checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISpanCheckpointRule
{
    List<string> Check(SpanCheckpoint item);
}

public sealed record SpanCheckpointQuantityRangeRule(int Min, int Max) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanCheckpointCodeLengthRule(int MaxLength) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanCheckpointWindowDaysRangeRule(int Min, int Max) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
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

public sealed record SpanCheckpointRequiredCodeRule(string Message) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanCheckpointCodePrefixRule(string Prefix) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanCheckpointScoreRangeRule(double Min, double Max) : ISpanCheckpointRule
{
    public List<string> Check(SpanCheckpoint item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span checkpoint.</summary>
public sealed record SpanCheckpointValidator(List<ISpanCheckpointRule> Rules)
{
    public List<string> Validate(SpanCheckpoint item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(SpanCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanCheckpoint> items)
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

/// <summary>Functions of span checkpoint rules.</summary>
public static class SpanCheckpointRules
{
    public static SpanCheckpointValidator DefaultSpanCheckpointValidator()
    {
        return new SpanCheckpointValidator(
            new List<ISpanCheckpointRule>
            {
                new SpanCheckpointQuantityRangeRule(16, 64),
                new SpanCheckpointCodeLengthRule(86400),
                new SpanCheckpointWindowDaysRangeRule(64, 120),
                new SpanCheckpointRequiredCodeRule("span checkpoint needs a value"),
                new SpanCheckpointCodePrefixRule("Q"),
                new SpanCheckpointScoreRangeRule(6.3, 2020.9),
            });
    }
}
