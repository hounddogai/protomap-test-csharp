namespace Acme.Workforce.Talent.Pool.Threshold;

/// <summary>A rule a strict pool plan must pass. Check returns its problems, or none.</summary>
public interface IStrictPoolPlanRule
{
    List<string> Check(StrictPoolPlan item);
}

public sealed record StrictPoolPlanEndsAtRangeRule(int Min, int Max) : IStrictPoolPlanRule
{
    public List<string> Check(StrictPoolPlan item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictPoolPlanLabelLengthRule(int MaxLength) : IStrictPoolPlanRule
{
    public List<string> Check(StrictPoolPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPoolPlanRetriesRangeRule(int Min, int Max) : IStrictPoolPlanRule
{
    public List<string> Check(StrictPoolPlan item)
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

/// <summary>Runs every rule of a strict pool plan.</summary>
public sealed record StrictPoolPlanValidator(List<IStrictPoolPlanRule> Rules)
{
    public List<string> Validate(StrictPoolPlan item)
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

    public void RequireValid(StrictPoolPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPoolPlan> items)
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

/// <summary>Functions of strict pool plan rules.</summary>
public static class StrictPoolPlanRules
{
    public static StrictPoolPlanValidator DefaultStrictPoolPlanValidator()
    {
        return new StrictPoolPlanValidator(
            new List<IStrictPoolPlanRule>
            {
                new StrictPoolPlanEndsAtRangeRule(1000, 86400),
                new StrictPoolPlanLabelLengthRule(1000),
                new StrictPoolPlanRetriesRangeRule(255, 3600),
            });
    }
}
