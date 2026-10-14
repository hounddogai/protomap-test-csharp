namespace Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>A rule a action plan audit must pass. Check returns its problems, or none.</summary>
public interface IActionPlanAuditRule
{
    List<string> Check(ActionPlanAudit item);
}

public sealed record ActionPlanAuditTitleLengthRule(int MaxLength) : IActionPlanAuditRule
{
    public List<string> Check(ActionPlanAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAuditRequiredTitleRule(string Message) : IActionPlanAuditRule
{
    public List<string> Check(ActionPlanAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAuditRequiredCurrencyRule(string Message) : IActionPlanAuditRule
{
    public List<string> Check(ActionPlanAudit item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAuditRequiredCountryRule(string Message) : IActionPlanAuditRule
{
    public List<string> Check(ActionPlanAudit item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan audit.</summary>
public sealed record ActionPlanAuditValidator(List<IActionPlanAuditRule> Rules)
{
    public List<string> Validate(ActionPlanAudit item)
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

    public void RequireValid(ActionPlanAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanAudit> items)
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

/// <summary>Functions of action plan audit rules.</summary>
public static class ActionPlanAuditRules
{
    public static ActionPlanAuditValidator DefaultActionPlanAuditValidator()
    {
        return new ActionPlanAuditValidator(
            new List<IActionPlanAuditRule>
            {
                new ActionPlanAuditTitleLengthRule(64),
                new ActionPlanAuditRequiredTitleRule("action plan audit needs a value"),
                new ActionPlanAuditRequiredCurrencyRule("action plan audit needs an entry"),
                new ActionPlanAuditRequiredCountryRule("action plan audit needs a value"),
            });
    }
}
