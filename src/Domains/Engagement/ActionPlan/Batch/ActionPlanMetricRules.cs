namespace Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>A rule a action plan metric must pass. Check returns its problems, or none.</summary>
public interface IActionPlanMetricRule
{
    List<string> Check(ActionPlanMetric item);
}

public sealed record ActionPlanMetricRequiredSkillRule(string Message) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMetricTitleLengthRule(int MaxLength) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMetricRevisionRangeRule(int Min, int Max) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMetricJobCodeLengthRule(int MaxLength) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMetricRequiredTitleRule(string Message) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMetricSkillLengthRule(int MaxLength) : IActionPlanMetricRule
{
    public List<string> Check(ActionPlanMetric item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan metric.</summary>
public sealed record ActionPlanMetricValidator(List<IActionPlanMetricRule> Rules)
{
    public List<string> Validate(ActionPlanMetric item)
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

    public void RequireValid(ActionPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanMetric> items)
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

/// <summary>Functions of action plan metric rules.</summary>
public static class ActionPlanMetricRules
{
    public static ActionPlanMetricValidator DefaultActionPlanMetricValidator()
    {
        return new ActionPlanMetricValidator(
            new List<IActionPlanMetricRule>
            {
                new ActionPlanMetricRequiredSkillRule("action plan metric needs an entry"),
                new ActionPlanMetricTitleLengthRule(64),
                new ActionPlanMetricRevisionRangeRule(64, 64),
                new ActionPlanMetricJobCodeLengthRule(32),
                new ActionPlanMetricRequiredTitleRule("action plan metric needs a value"),
                new ActionPlanMetricSkillLengthRule(64),
            });
    }
}
