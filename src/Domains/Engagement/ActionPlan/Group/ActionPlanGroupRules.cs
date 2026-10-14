namespace Acme.Workforce.Engagement.ActionPlan.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan group must pass. Check returns its problems, or none.</summary>
public interface IActionPlanGroupRule
{
    List<string> Check(ActionPlanGroup item);
}

public sealed record ActionPlanGroupTeamLengthRule(int MaxLength) : IActionPlanGroupRule
{
    public List<string> Check(ActionPlanGroup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanGroupAllowedTeamRule(List<string> Allowed) : IActionPlanGroupRule
{
    public List<string> Check(ActionPlanGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanGroupSummaryLengthRule(int MaxLength) : IActionPlanGroupRule
{
    public List<string> Check(ActionPlanGroup item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan group.</summary>
public sealed record ActionPlanGroupValidator(List<IActionPlanGroupRule> Rules)
{
    public List<string> Validate(ActionPlanGroup item)
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

    public void RequireValid(ActionPlanGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanGroup> items)
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

/// <summary>Functions of action plan group rules.</summary>
public static class ActionPlanGroupRules
{
    public static ActionPlanGroupValidator DefaultActionPlanGroupValidator()
    {
        return new ActionPlanGroupValidator(
            new List<IActionPlanGroupRule>
            {
                new ActionPlanGroupTeamLengthRule(255),
                new ActionPlanGroupAllowedTeamRule(new List<string>()),
                new ActionPlanGroupSummaryLengthRule(255),
            });
    }
}
