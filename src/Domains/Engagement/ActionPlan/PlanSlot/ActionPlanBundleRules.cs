namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>A rule a action plan bundle must pass. Check returns its problems, or none.</summary>
public interface IActionPlanBundleRule
{
    List<string> Check(ActionPlanBundle item);
}

public sealed record ActionPlanBundleCapacityRangeRule(int Min, int Max) : IActionPlanBundleRule
{
    public List<string> Check(ActionPlanBundle item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBundleRequiredDescriptionRule(string Message) : IActionPlanBundleRule
{
    public List<string> Check(ActionPlanBundle item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBundleDescriptionLengthRule(int MaxLength) : IActionPlanBundleRule
{
    public List<string> Check(ActionPlanBundle item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBundleRequiredNameRule(string Message) : IActionPlanBundleRule
{
    public List<string> Check(ActionPlanBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBundleRequiredCodeRule(string Message) : IActionPlanBundleRule
{
    public List<string> Check(ActionPlanBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan bundle.</summary>
public sealed record ActionPlanBundleValidator(List<IActionPlanBundleRule> Rules)
{
    public List<string> Validate(ActionPlanBundle item)
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

    public void RequireValid(ActionPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanBundle> items)
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

/// <summary>Functions of action plan bundle rules.</summary>
public static class ActionPlanBundleRules
{
    public static ActionPlanBundleValidator DefaultActionPlanBundleValidator()
    {
        return new ActionPlanBundleValidator(
            new List<IActionPlanBundleRule>
            {
                new ActionPlanBundleCapacityRangeRule(255, 3600),
                new ActionPlanBundleRequiredDescriptionRule("action plan bundle needs a value"),
                new ActionPlanBundleDescriptionLengthRule(3600),
                new ActionPlanBundleRequiredNameRule("action plan bundle needs this field"),
                new ActionPlanBundleRequiredCodeRule("action plan bundle needs this field"),
            });
    }
}
