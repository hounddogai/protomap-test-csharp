namespace Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>A rule a action plan cycle must pass. Check returns its problems, or none.</summary>
public interface IActionPlanCycleRule
{
    List<string> Check(ActionPlanCycle item);
}

public sealed record ActionPlanCycleCodeLengthRule(int MaxLength) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanCycleCodePrefixRule(string Prefix) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanCycleContactEmailLengthRule(int MaxLength) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanCycleTeamLengthRule(int MaxLength) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanCycleRequiredNameRule(string Message) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanCycleRequiredTeamRule(string Message) : IActionPlanCycleRule
{
    public List<string> Check(ActionPlanCycle item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan cycle.</summary>
public sealed record ActionPlanCycleValidator(List<IActionPlanCycleRule> Rules)
{
    public List<string> Validate(ActionPlanCycle item)
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

    public void RequireValid(ActionPlanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanCycle> items)
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

/// <summary>Functions of action plan cycle rules.</summary>
public static class ActionPlanCycleRules
{
    public static ActionPlanCycleValidator DefaultActionPlanCycleValidator()
    {
        return new ActionPlanCycleValidator(
            new List<IActionPlanCycleRule>
            {
                new ActionPlanCycleCodeLengthRule(80),
                new ActionPlanCycleCodePrefixRule("Q"),
                new ActionPlanCycleContactEmailLengthRule(86400),
                new ActionPlanCycleTeamLengthRule(1000),
                new ActionPlanCycleRequiredNameRule("action plan cycle needs this field"),
                new ActionPlanCycleRequiredTeamRule("action plan cycle needs this field"),
            });
    }
}
