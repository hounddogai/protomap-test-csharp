namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>A rule a action plan summary must pass. Check returns its problems, or none.</summary>
public interface IActionPlanSummaryRule
{
    List<string> Check(ActionPlanSummary item);
}

public sealed record ActionPlanSummaryAttemptsRangeRule(int Min, int Max) : IActionPlanSummaryRule
{
    public List<string> Check(ActionPlanSummary item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanSummarySequenceRangeRule(int Min, int Max) : IActionPlanSummaryRule
{
    public List<string> Check(ActionPlanSummary item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSummaryCodePrefixRule(string Prefix) : IActionPlanSummaryRule
{
    public List<string> Check(ActionPlanSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan summary.</summary>
public sealed record ActionPlanSummaryValidator(List<IActionPlanSummaryRule> Rules)
{
    public List<string> Validate(ActionPlanSummary item)
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

    public void RequireValid(ActionPlanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanSummary> items)
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

/// <summary>Functions of action plan summary rules.</summary>
public static class ActionPlanSummaryRules
{
    public static ActionPlanSummaryValidator DefaultActionPlanSummaryValidator()
    {
        return new ActionPlanSummaryValidator(
            new List<IActionPlanSummaryRule>
            {
                new ActionPlanSummaryAttemptsRangeRule(120, 1000),
                new ActionPlanSummarySequenceRangeRule(32, 255),
                new ActionPlanSummaryCodePrefixRule("REF"),
            });
    }
}
