namespace Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>A rule a action plan claim must pass. Check returns its problems, or none.</summary>
public interface IActionPlanClaimRule
{
    List<string> Check(ActionPlanClaim item);
}

public sealed record ActionPlanClaimRequiredCodeRule(string Message) : IActionPlanClaimRule
{
    public List<string> Check(ActionPlanClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanClaimCeilingRangeRule(double Min, double Max) : IActionPlanClaimRule
{
    public List<string> Check(ActionPlanClaim item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanClaimHoursRangeRule(double Min, double Max) : IActionPlanClaimRule
{
    public List<string> Check(ActionPlanClaim item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanClaimRequiredNameRule(string Message) : IActionPlanClaimRule
{
    public List<string> Check(ActionPlanClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan claim.</summary>
public sealed record ActionPlanClaimValidator(List<IActionPlanClaimRule> Rules)
{
    public List<string> Validate(ActionPlanClaim item)
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

    public void RequireValid(ActionPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanClaim> items)
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

/// <summary>Functions of action plan claim rules.</summary>
public static class ActionPlanClaimRules
{
    public static ActionPlanClaimValidator DefaultActionPlanClaimValidator()
    {
        return new ActionPlanClaimValidator(
            new List<IActionPlanClaimRule>
            {
                new ActionPlanClaimRequiredCodeRule("action plan claim needs an entry"),
                new ActionPlanClaimCeilingRangeRule(3.5, 4840.6),
                new ActionPlanClaimHoursRangeRule(2.6, 3360.1),
                new ActionPlanClaimRequiredNameRule("action plan claim needs an entry"),
            });
    }
}
