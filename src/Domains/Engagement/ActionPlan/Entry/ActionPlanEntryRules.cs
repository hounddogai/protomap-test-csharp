namespace Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>A rule a action plan entry must pass. Check returns its problems, or none.</summary>
public interface IActionPlanEntryRule
{
    List<string> Check(ActionPlanEntry item);
}

public sealed record ActionPlanEntryRequiredCodeRule(string Message) : IActionPlanEntryRule
{
    public List<string> Check(ActionPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanEntryRetriesRangeRule(int Min, int Max) : IActionPlanEntryRule
{
    public List<string> Check(ActionPlanEntry item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanEntryRequiredLabelRule(string Message) : IActionPlanEntryRule
{
    public List<string> Check(ActionPlanEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan entry.</summary>
public sealed record ActionPlanEntryValidator(List<IActionPlanEntryRule> Rules)
{
    public List<string> Validate(ActionPlanEntry item)
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

    public void RequireValid(ActionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanEntry> items)
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

/// <summary>Functions of action plan entry rules.</summary>
public static class ActionPlanEntryRules
{
    public static ActionPlanEntryValidator DefaultActionPlanEntryValidator()
    {
        return new ActionPlanEntryValidator(
            new List<IActionPlanEntryRule>
            {
                new ActionPlanEntryRequiredCodeRule("action plan entry needs this field"),
                new ActionPlanEntryRetriesRangeRule(64, 120),
                new ActionPlanEntryRequiredLabelRule("action plan entry needs an entry"),
            });
    }
}
