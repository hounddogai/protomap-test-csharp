namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>A rule a action plan variance must pass. Check returns its problems, or none.</summary>
public interface IActionPlanVarianceRule
{
    List<string> Check(ActionPlanVariance item);
}

public sealed record ActionPlanVarianceCodeLengthRule(int MaxLength) : IActionPlanVarianceRule
{
    public List<string> Check(ActionPlanVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanVarianceRequiredOwnerIdRule(string Message) : IActionPlanVarianceRule
{
    public List<string> Check(ActionPlanVariance item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanVarianceRequiredLabelRule(string Message) : IActionPlanVarianceRule
{
    public List<string> Check(ActionPlanVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan variance.</summary>
public sealed record ActionPlanVarianceValidator(List<IActionPlanVarianceRule> Rules)
{
    public List<string> Validate(ActionPlanVariance item)
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

    public void RequireValid(ActionPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanVariance> items)
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

/// <summary>Functions of action plan variance rules.</summary>
public static class ActionPlanVarianceRules
{
    public static ActionPlanVarianceValidator DefaultActionPlanVarianceValidator()
    {
        return new ActionPlanVarianceValidator(
            new List<IActionPlanVarianceRule>
            {
                new ActionPlanVarianceCodeLengthRule(86400),
                new ActionPlanVarianceRequiredOwnerIdRule("action plan variance needs this field"),
                new ActionPlanVarianceRequiredLabelRule("action plan variance needs this field"),
            });
    }
}
