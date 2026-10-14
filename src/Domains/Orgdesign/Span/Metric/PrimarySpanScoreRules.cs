namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>A rule a primary span score must pass. Check returns its problems, or none.</summary>
public interface IPrimarySpanScoreRule
{
    List<string> Check(PrimarySpanScore item);
}

public sealed record PrimarySpanScoreRequiredCodeRule(string Message) : IPrimarySpanScoreRule
{
    public List<string> Check(PrimarySpanScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySpanScoreRetriesRangeRule(int Min, int Max) : IPrimarySpanScoreRule
{
    public List<string> Check(PrimarySpanScore item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimarySpanScoreCodePrefixRule(string Prefix) : IPrimarySpanScoreRule
{
    public List<string> Check(PrimarySpanScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySpanScoreLabelLengthRule(int MaxLength) : IPrimarySpanScoreRule
{
    public List<string> Check(PrimarySpanScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySpanScoreCodeLengthRule(int MaxLength) : IPrimarySpanScoreRule
{
    public List<string> Check(PrimarySpanScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary span score.</summary>
public sealed record PrimarySpanScoreValidator(List<IPrimarySpanScoreRule> Rules)
{
    public List<string> Validate(PrimarySpanScore item)
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

    public void RequireValid(PrimarySpanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySpanScore> items)
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

/// <summary>Functions of primary span score rules.</summary>
public static class PrimarySpanScoreRules
{
    public static PrimarySpanScoreValidator DefaultPrimarySpanScoreValidator()
    {
        return new PrimarySpanScoreValidator(
            new List<IPrimarySpanScoreRule>
            {
                new PrimarySpanScoreRequiredCodeRule("primary span score needs this field"),
                new PrimarySpanScoreRetriesRangeRule(255, 86400),
                new PrimarySpanScoreCodePrefixRule("X"),
                new PrimarySpanScoreLabelLengthRule(16),
                new PrimarySpanScoreCodeLengthRule(86400),
            });
    }
}
