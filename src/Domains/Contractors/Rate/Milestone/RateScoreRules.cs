namespace Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>A rule a rate score must pass. Check returns its problems, or none.</summary>
public interface IRateScoreRule
{
    List<string> Check(RateScore item);
}

public sealed record RateScoreFteRangeRule(double Min, double Max) : IRateScoreRule
{
    public List<string> Check(RateScore item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateScorePriorityRangeRule(int Min, int Max) : IRateScoreRule
{
    public List<string> Check(RateScore item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateScoreCostRangeRule(double Min, double Max) : IRateScoreRule
{
    public List<string> Check(RateScore item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateScoreCodePrefixRule(string Prefix) : IRateScoreRule
{
    public List<string> Check(RateScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate score.</summary>
public sealed record RateScoreValidator(List<IRateScoreRule> Rules)
{
    public List<string> Validate(RateScore item)
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

    public void RequireValid(RateScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateScore> items)
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

/// <summary>Functions of rate score rules.</summary>
public static class RateScoreRules
{
    public static RateScoreValidator DefaultRateScoreValidator()
    {
        return new RateScoreValidator(
            new List<IRateScoreRule>
            {
                new RateScoreFteRangeRule(9.3, 1548.9),
                new RateScorePriorityRangeRule(32, 64),
                new RateScoreCostRangeRule(6.2, 1964.3),
                new RateScoreCodePrefixRule("REF"),
            });
    }
}
