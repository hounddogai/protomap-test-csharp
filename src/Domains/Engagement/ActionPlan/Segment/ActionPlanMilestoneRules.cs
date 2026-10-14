namespace Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>A rule a action plan milestone must pass. Check returns its problems, or none.</summary>
public interface IActionPlanMilestoneRule
{
    List<string> Check(ActionPlanMilestone item);
}

public sealed record ActionPlanMilestoneEndsAtRangeRule(int Min, int Max) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
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

public sealed record ActionPlanMilestoneRevisionRangeRule(int Min, int Max) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMilestoneLabelLengthRule(int MaxLength) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMilestoneRequiredCodeRule(string Message) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMilestoneSourceLengthRule(int MaxLength) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMilestoneRequiredLabelRule(string Message) : IActionPlanMilestoneRule
{
    public List<string> Check(ActionPlanMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan milestone.</summary>
public sealed record ActionPlanMilestoneValidator(List<IActionPlanMilestoneRule> Rules)
{
    public List<string> Validate(ActionPlanMilestone item)
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

    public void RequireValid(ActionPlanMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanMilestone> items)
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

/// <summary>Functions of action plan milestone rules.</summary>
public static class ActionPlanMilestoneRules
{
    public static ActionPlanMilestoneValidator DefaultActionPlanMilestoneValidator()
    {
        return new ActionPlanMilestoneValidator(
            new List<IActionPlanMilestoneRule>
            {
                new ActionPlanMilestoneEndsAtRangeRule(3600, 3600),
                new ActionPlanMilestoneRevisionRangeRule(3600, 86400),
                new ActionPlanMilestoneLabelLengthRule(80),
                new ActionPlanMilestoneRequiredCodeRule("action plan milestone needs an entry"),
                new ActionPlanMilestoneSourceLengthRule(32),
                new ActionPlanMilestoneRequiredLabelRule("action plan milestone needs a value"),
            });
    }
}
