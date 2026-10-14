namespace Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>A rule a mobility score must pass. Check returns its problems, or none.</summary>
public interface IMobilityScoreRule
{
    List<string> Check(MobilityScore item);
}

public sealed record MobilityScoreBatchSizeRangeRule(int Min, int Max) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityScoreCodeLengthRule(int MaxLength) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityScoreCodePrefixRule(string Prefix) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityScoreRequiredTitleRule(string Message) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityScoreVersionRangeRule(int Min, int Max) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityScoreTitleLengthRule(int MaxLength) : IMobilityScoreRule
{
    public List<string> Check(MobilityScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility score.</summary>
public sealed record MobilityScoreValidator(List<IMobilityScoreRule> Rules)
{
    public List<string> Validate(MobilityScore item)
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

    public void RequireValid(MobilityScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityScore> items)
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

/// <summary>Functions of mobility score rules.</summary>
public static class MobilityScoreRules
{
    public static MobilityScoreValidator DefaultMobilityScoreValidator()
    {
        return new MobilityScoreValidator(
            new List<IMobilityScoreRule>
            {
                new MobilityScoreBatchSizeRangeRule(80, 1000),
                new MobilityScoreCodeLengthRule(3600),
                new MobilityScoreCodePrefixRule("OPS"),
                new MobilityScoreRequiredTitleRule("mobility score needs a value"),
                new MobilityScoreVersionRangeRule(16, 3600),
                new MobilityScoreTitleLengthRule(80),
            });
    }
}
