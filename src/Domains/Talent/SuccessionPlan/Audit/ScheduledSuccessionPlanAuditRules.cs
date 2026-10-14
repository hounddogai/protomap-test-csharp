namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled succession plan audit must pass. Check returns its problems, or none.</summary>
public interface IScheduledSuccessionPlanAuditRule
{
    List<string> Check(ScheduledSuccessionPlanAudit item);
}

public sealed record ScheduledSuccessionPlanAuditRequiredCostCenterRule(string Message) : IScheduledSuccessionPlanAuditRule
{
    public List<string> Check(ScheduledSuccessionPlanAudit item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSuccessionPlanAuditAllowedTitleRule(List<string> Allowed) : IScheduledSuccessionPlanAuditRule
{
    public List<string> Check(ScheduledSuccessionPlanAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSuccessionPlanAuditCodeLengthRule(int MaxLength) : IScheduledSuccessionPlanAuditRule
{
    public List<string> Check(ScheduledSuccessionPlanAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled succession plan audit.</summary>
public sealed record ScheduledSuccessionPlanAuditValidator(List<IScheduledSuccessionPlanAuditRule> Rules)
{
    public List<string> Validate(ScheduledSuccessionPlanAudit item)
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

    public void RequireValid(ScheduledSuccessionPlanAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSuccessionPlanAudit> items)
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

/// <summary>Functions of scheduled succession plan audit rules.</summary>
public static class ScheduledSuccessionPlanAuditRules
{
    public static ScheduledSuccessionPlanAuditValidator DefaultScheduledSuccessionPlanAuditValidator()
    {
        return new ScheduledSuccessionPlanAuditValidator(
            new List<IScheduledSuccessionPlanAuditRule>
            {
                new ScheduledSuccessionPlanAuditRequiredCostCenterRule(
                    "scheduled succession plan audit needs this field"),
                new ScheduledSuccessionPlanAuditAllowedTitleRule(new List<string> { "scheduled" }),
                new ScheduledSuccessionPlanAuditCodeLengthRule(80),
            });
    }
}
