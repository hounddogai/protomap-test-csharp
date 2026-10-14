namespace Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>A rule a seasonal engagement rollup must pass. Check returns its problems, or none.</summary>
public interface ISeasonalEngagementRollupRule
{
    List<string> Check(SeasonalEngagementRollup item);
}

public sealed record SeasonalEngagementRollupRequiredCodeRule(string Message) : ISeasonalEngagementRollupRule
{
    public List<string> Check(SeasonalEngagementRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementRollupRetriesRangeRule(int Min, int Max) : ISeasonalEngagementRollupRule
{
    public List<string> Check(SeasonalEngagementRollup item)
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

public sealed record SeasonalEngagementRollupRequiredJobCodeRule(string Message) : ISeasonalEngagementRollupRule
{
    public List<string> Check(SeasonalEngagementRollup item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementRollupGraceDaysRangeRule(int Min, int Max) : ISeasonalEngagementRollupRule
{
    public List<string> Check(SeasonalEngagementRollup item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal engagement rollup.</summary>
public sealed record SeasonalEngagementRollupValidator(List<ISeasonalEngagementRollupRule> Rules)
{
    public List<string> Validate(SeasonalEngagementRollup item)
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

    public void RequireValid(SeasonalEngagementRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalEngagementRollup> items)
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

/// <summary>Functions of seasonal engagement rollup rules.</summary>
public static class SeasonalEngagementRollupRules
{
    public static SeasonalEngagementRollupValidator DefaultSeasonalEngagementRollupValidator()
    {
        return new SeasonalEngagementRollupValidator(
            new List<ISeasonalEngagementRollupRule>
            {
                new SeasonalEngagementRollupRequiredCodeRule("seasonal engagement rollup needs a value"),
                new SeasonalEngagementRollupRetriesRangeRule(16, 80),
                new SeasonalEngagementRollupRequiredJobCodeRule("seasonal engagement rollup needs this field"),
                new SeasonalEngagementRollupGraceDaysRangeRule(64, 1000),
            });
    }
}
