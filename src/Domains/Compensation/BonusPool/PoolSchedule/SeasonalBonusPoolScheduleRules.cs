namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>A rule a seasonal bonus pool schedule must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBonusPoolScheduleRule
{
    List<string> Check(SeasonalBonusPoolSchedule item);
}

public sealed record SeasonalBonusPoolScheduleCostRangeRule(double Min, double Max) : ISeasonalBonusPoolScheduleRule
{
    public List<string> Check(SeasonalBonusPoolSchedule item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBonusPoolScheduleRequiredReviewerRule(string Message) : ISeasonalBonusPoolScheduleRule
{
    public List<string> Check(SeasonalBonusPoolSchedule item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBonusPoolScheduleCodePrefixRule(string Prefix) : ISeasonalBonusPoolScheduleRule
{
    public List<string> Check(SeasonalBonusPoolSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBonusPoolSchedulePriorityRangeRule(int Min, int Max) : ISeasonalBonusPoolScheduleRule
{
    public List<string> Check(SeasonalBonusPoolSchedule item)
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

public sealed record SeasonalBonusPoolScheduleTitleLengthRule(int MaxLength) : ISeasonalBonusPoolScheduleRule
{
    public List<string> Check(SeasonalBonusPoolSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal bonus pool schedule.</summary>
public sealed record SeasonalBonusPoolScheduleValidator(List<ISeasonalBonusPoolScheduleRule> Rules)
{
    public List<string> Validate(SeasonalBonusPoolSchedule item)
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

    public void RequireValid(SeasonalBonusPoolSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBonusPoolSchedule> items)
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

/// <summary>Functions of seasonal bonus pool schedule rules.</summary>
public static class SeasonalBonusPoolScheduleRules
{
    public static SeasonalBonusPoolScheduleValidator DefaultSeasonalBonusPoolScheduleValidator()
    {
        return new SeasonalBonusPoolScheduleValidator(
            new List<ISeasonalBonusPoolScheduleRule>
            {
                new SeasonalBonusPoolScheduleCostRangeRule(0.5, 2765.5),
                new SeasonalBonusPoolScheduleRequiredReviewerRule("seasonal bonus pool schedule needs a value"),
                new SeasonalBonusPoolScheduleCodePrefixRule("Q"),
                new SeasonalBonusPoolSchedulePriorityRangeRule(120, 3600),
                new SeasonalBonusPoolScheduleTitleLengthRule(1000),
            });
    }
}
