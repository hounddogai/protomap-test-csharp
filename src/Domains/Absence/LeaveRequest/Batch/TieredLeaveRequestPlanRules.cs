namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered leave request plan must pass. Check returns its problems, or none.</summary>
public interface ITieredLeaveRequestPlanRule
{
    List<string> Check(TieredLeaveRequestPlan item);
}

public sealed record TieredLeaveRequestPlanRequiredLabelRule(string Message) : ITieredLeaveRequestPlanRule
{
    public List<string> Check(TieredLeaveRequestPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestPlanReviewerLengthRule(int MaxLength) : ITieredLeaveRequestPlanRule
{
    public List<string> Check(TieredLeaveRequestPlan item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestPlanAllowedSkillRule(List<string> Allowed) : ITieredLeaveRequestPlanRule
{
    public List<string> Check(TieredLeaveRequestPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestPlanCodeLengthRule(int MaxLength) : ITieredLeaveRequestPlanRule
{
    public List<string> Check(TieredLeaveRequestPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered leave request plan.</summary>
public sealed record TieredLeaveRequestPlanValidator(List<ITieredLeaveRequestPlanRule> Rules)
{
    public List<string> Validate(TieredLeaveRequestPlan item)
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

    public void RequireValid(TieredLeaveRequestPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLeaveRequestPlan> items)
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

/// <summary>Functions of tiered leave request plan rules.</summary>
public static class TieredLeaveRequestPlanRules
{
    public static TieredLeaveRequestPlanValidator DefaultTieredLeaveRequestPlanValidator()
    {
        return new TieredLeaveRequestPlanValidator(
            new List<ITieredLeaveRequestPlanRule>
            {
                new TieredLeaveRequestPlanRequiredLabelRule("tiered leave request plan needs a value"),
                new TieredLeaveRequestPlanReviewerLengthRule(16),
                new TieredLeaveRequestPlanAllowedSkillRule(new List<string> { "manual" }),
                new TieredLeaveRequestPlanCodeLengthRule(64),
            });
    }
}
