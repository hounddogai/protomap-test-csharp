namespace Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>A rule a action plan profile must pass. Check returns its problems, or none.</summary>
public interface IActionPlanProfileRule
{
    List<string> Check(ActionPlanProfile item);
}

public sealed record ActionPlanProfileCodeLengthRule(int MaxLength) : IActionPlanProfileRule
{
    public List<string> Check(ActionPlanProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanProfileRequiredCodeRule(string Message) : IActionPlanProfileRule
{
    public List<string> Check(ActionPlanProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanProfileTitleLengthRule(int MaxLength) : IActionPlanProfileRule
{
    public List<string> Check(ActionPlanProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanProfileRequiredManagerIdRule(string Message) : IActionPlanProfileRule
{
    public List<string> Check(ActionPlanProfile item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan profile.</summary>
public sealed record ActionPlanProfileValidator(List<IActionPlanProfileRule> Rules)
{
    public List<string> Validate(ActionPlanProfile item)
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

    public void RequireValid(ActionPlanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanProfile> items)
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

/// <summary>Functions of action plan profile rules.</summary>
public static class ActionPlanProfileRules
{
    public static ActionPlanProfileValidator DefaultActionPlanProfileValidator()
    {
        return new ActionPlanProfileValidator(
            new List<IActionPlanProfileRule>
            {
                new ActionPlanProfileCodeLengthRule(64),
                new ActionPlanProfileRequiredCodeRule("action plan profile needs this field"),
                new ActionPlanProfileTitleLengthRule(1000),
                new ActionPlanProfileRequiredManagerIdRule("action plan profile needs an entry"),
            });
    }
}
