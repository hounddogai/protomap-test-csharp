namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>A rule a action plan exception must pass. Check returns its problems, or none.</summary>
public interface IActionPlanExceptionRule
{
    List<string> Check(ActionPlanException item);
}

public sealed record ActionPlanExceptionMarginRangeRule(double Min, double Max) : IActionPlanExceptionRule
{
    public List<string> Check(ActionPlanException item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IActionPlanExceptionRule
{
    public List<string> Check(ActionPlanException item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ActionPlanExceptionCodePrefixRule(string Prefix) : IActionPlanExceptionRule
{
    public List<string> Check(ActionPlanException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanExceptionThresholdRangeRule(double Min, double Max) : IActionPlanExceptionRule
{
    public List<string> Check(ActionPlanException item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a action plan exception.</summary>
public sealed record ActionPlanExceptionValidator(List<IActionPlanExceptionRule> Rules)
{
    public List<string> Validate(ActionPlanException item)
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

    public void RequireValid(ActionPlanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanException> items)
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

/// <summary>Functions of action plan exception rules.</summary>
public static class ActionPlanExceptionRules
{
    public static ActionPlanExceptionValidator DefaultActionPlanExceptionValidator()
    {
        return new ActionPlanExceptionValidator(
            new List<IActionPlanExceptionRule>
            {
                new ActionPlanExceptionMarginRangeRule(1.1, 741.7),
                new ActionPlanExceptionTagsLimitRule(80, new List<string> { "adaptive", "batched", "seasonal" }),
                new ActionPlanExceptionCodePrefixRule("ACM"),
                new ActionPlanExceptionThresholdRangeRule(6.8, 1343.9),
            });
    }
}
