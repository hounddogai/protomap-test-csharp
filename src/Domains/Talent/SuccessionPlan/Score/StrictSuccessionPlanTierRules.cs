namespace Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>A rule a strict succession plan tier must pass. Check returns its problems, or none.</summary>
public interface IStrictSuccessionPlanTierRule
{
    List<string> Check(StrictSuccessionPlanTier item);
}

public sealed record StrictSuccessionPlanTierFteRangeRule(double Min, double Max) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanTierCeilingRangeRule(double Min, double Max) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanTierRequiredNameRule(string Message) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanTierCapacityRangeRule(int Min, int Max) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanTierCodeLengthRule(int MaxLength) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanTierBudgetRangeRule(double Min, double Max) : IStrictSuccessionPlanTierRule
{
    public List<string> Check(StrictSuccessionPlanTier item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict succession plan tier.</summary>
public sealed record StrictSuccessionPlanTierValidator(List<IStrictSuccessionPlanTierRule> Rules)
{
    public List<string> Validate(StrictSuccessionPlanTier item)
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

    public void RequireValid(StrictSuccessionPlanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSuccessionPlanTier> items)
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

/// <summary>Functions of strict succession plan tier rules.</summary>
public static class StrictSuccessionPlanTierRules
{
    public static StrictSuccessionPlanTierValidator DefaultStrictSuccessionPlanTierValidator()
    {
        return new StrictSuccessionPlanTierValidator(
            new List<IStrictSuccessionPlanTierRule>
            {
                new StrictSuccessionPlanTierFteRangeRule(5.6, 2047.5),
                new StrictSuccessionPlanTierCeilingRangeRule(1.6, 3875.2),
                new StrictSuccessionPlanTierRequiredNameRule("strict succession plan tier needs this field"),
                new StrictSuccessionPlanTierCapacityRangeRule(64, 3600),
                new StrictSuccessionPlanTierCodeLengthRule(255),
                new StrictSuccessionPlanTierBudgetRangeRule(7.7, 3358.1),
            });
    }
}
