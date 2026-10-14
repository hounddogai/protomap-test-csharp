namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan quota must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanQuotaRule
{
    List<string> Check(SuccessionPlanQuota item);
}

public sealed record SuccessionPlanQuotaAllowedCodeRule(List<string> Allowed) : ISuccessionPlanQuotaRule
{
    public List<string> Check(SuccessionPlanQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanQuotaRequiredCodeRule(string Message) : ISuccessionPlanQuotaRule
{
    public List<string> Check(SuccessionPlanQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanQuotaRequiredDepartmentRule(string Message) : ISuccessionPlanQuotaRule
{
    public List<string> Check(SuccessionPlanQuota item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan quota.</summary>
public sealed record SuccessionPlanQuotaValidator(List<ISuccessionPlanQuotaRule> Rules)
{
    public List<string> Validate(SuccessionPlanQuota item)
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

    public void RequireValid(SuccessionPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanQuota> items)
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

/// <summary>Functions of succession plan quota rules.</summary>
public static class SuccessionPlanQuotaRules
{
    public static SuccessionPlanQuotaValidator DefaultSuccessionPlanQuotaValidator()
    {
        return new SuccessionPlanQuotaValidator(
            new List<ISuccessionPlanQuotaRule>
            {
                new SuccessionPlanQuotaAllowedCodeRule(new List<string> { "regional", "fallback", "tiered" }),
                new SuccessionPlanQuotaRequiredCodeRule("succession plan quota needs an entry"),
                new SuccessionPlanQuotaRequiredDepartmentRule("succession plan quota needs a value"),
            });
    }
}
