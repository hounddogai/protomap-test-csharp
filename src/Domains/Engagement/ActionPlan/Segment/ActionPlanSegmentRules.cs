namespace Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>A rule a action plan segment must pass. Check returns its problems, or none.</summary>
public interface IActionPlanSegmentRule
{
    List<string> Check(ActionPlanSegment item);
}

public sealed record ActionPlanSegmentRequiredCodeRule(string Message) : IActionPlanSegmentRule
{
    public List<string> Check(ActionPlanSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSegmentRequiredTitleRule(string Message) : IActionPlanSegmentRule
{
    public List<string> Check(ActionPlanSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSegmentCodePrefixRule(string Prefix) : IActionPlanSegmentRule
{
    public List<string> Check(ActionPlanSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSegmentRequiredChannelRule(string Message) : IActionPlanSegmentRule
{
    public List<string> Check(ActionPlanSegment item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSegmentRequiredTimezoneRule(string Message) : IActionPlanSegmentRule
{
    public List<string> Check(ActionPlanSegment item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan segment.</summary>
public sealed record ActionPlanSegmentValidator(List<IActionPlanSegmentRule> Rules)
{
    public List<string> Validate(ActionPlanSegment item)
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

    public void RequireValid(ActionPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanSegment> items)
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

/// <summary>Functions of action plan segment rules.</summary>
public static class ActionPlanSegmentRules
{
    public static ActionPlanSegmentValidator DefaultActionPlanSegmentValidator()
    {
        return new ActionPlanSegmentValidator(
            new List<IActionPlanSegmentRule>
            {
                new ActionPlanSegmentRequiredCodeRule("action plan segment needs a value"),
                new ActionPlanSegmentRequiredTitleRule("action plan segment needs an entry"),
                new ActionPlanSegmentCodePrefixRule("ACM"),
                new ActionPlanSegmentRequiredChannelRule("action plan segment needs this field"),
                new ActionPlanSegmentRequiredTimezoneRule("action plan segment needs an entry"),
            });
    }
}
