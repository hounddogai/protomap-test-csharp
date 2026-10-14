namespace Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>A rule a action plan request must pass. Check returns its problems, or none.</summary>
public interface IActionPlanRequestRule
{
    List<string> Check(ActionPlanRequest item);
}

public sealed record ActionPlanRequestRequiredNameRule(string Message) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRequestNameLengthRule(int MaxLength) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRequestCodeLengthRule(int MaxLength) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRequestAttemptsRangeRule(int Min, int Max) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
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

public sealed record ActionPlanRequestVersionRangeRule(int Min, int Max) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanRequestDepartmentLengthRule(int MaxLength) : IActionPlanRequestRule
{
    public List<string> Check(ActionPlanRequest item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan request.</summary>
public sealed record ActionPlanRequestValidator(List<IActionPlanRequestRule> Rules)
{
    public List<string> Validate(ActionPlanRequest item)
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

    public void RequireValid(ActionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanRequest> items)
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

/// <summary>Functions of action plan request rules.</summary>
public static class ActionPlanRequestRules
{
    public static ActionPlanRequestValidator DefaultActionPlanRequestValidator()
    {
        return new ActionPlanRequestValidator(
            new List<IActionPlanRequestRule>
            {
                new ActionPlanRequestRequiredNameRule("action plan request needs an entry"),
                new ActionPlanRequestNameLengthRule(32),
                new ActionPlanRequestCodeLengthRule(64),
                new ActionPlanRequestAttemptsRangeRule(255, 1000),
                new ActionPlanRequestVersionRangeRule(16, 80),
                new ActionPlanRequestDepartmentLengthRule(16),
            });
    }
}
