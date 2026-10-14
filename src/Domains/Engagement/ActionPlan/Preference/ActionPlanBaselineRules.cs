namespace Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>A rule a action plan baseline must pass. Check returns its problems, or none.</summary>
public interface IActionPlanBaselineRule
{
    List<string> Check(ActionPlanBaseline item);
}

public sealed record ActionPlanBaselineSummaryLengthRule(int MaxLength) : IActionPlanBaselineRule
{
    public List<string> Check(ActionPlanBaseline item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBaselineRequiredCodeRule(string Message) : IActionPlanBaselineRule
{
    public List<string> Check(ActionPlanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBaselineRequiredLabelRule(string Message) : IActionPlanBaselineRule
{
    public List<string> Check(ActionPlanBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan baseline.</summary>
public sealed record ActionPlanBaselineValidator(List<IActionPlanBaselineRule> Rules)
{
    public List<string> Validate(ActionPlanBaseline item)
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

    public void RequireValid(ActionPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanBaseline> items)
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

/// <summary>Functions of action plan baseline rules.</summary>
public static class ActionPlanBaselineRules
{
    public static ActionPlanBaselineValidator DefaultActionPlanBaselineValidator()
    {
        return new ActionPlanBaselineValidator(
            new List<IActionPlanBaselineRule>
            {
                new ActionPlanBaselineSummaryLengthRule(86400),
                new ActionPlanBaselineRequiredCodeRule("action plan baseline needs an entry"),
                new ActionPlanBaselineRequiredLabelRule("action plan baseline needs this field"),
            });
    }
}
