namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default return plan quota must pass. Check returns its problems, or none.</summary>
public interface IDefaultReturnPlanQuotaRule
{
    List<string> Check(DefaultReturnPlanQuota item);
}

public sealed record DefaultReturnPlanQuotaCodeLengthRule(int MaxLength) : IDefaultReturnPlanQuotaRule
{
    public List<string> Check(DefaultReturnPlanQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanQuotaAllowedCodeRule(List<string> Allowed) : IDefaultReturnPlanQuotaRule
{
    public List<string> Check(DefaultReturnPlanQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanQuotaRequiredCodeRule(string Message) : IDefaultReturnPlanQuotaRule
{
    public List<string> Check(DefaultReturnPlanQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default return plan quota.</summary>
public sealed record DefaultReturnPlanQuotaValidator(List<IDefaultReturnPlanQuotaRule> Rules)
{
    public List<string> Validate(DefaultReturnPlanQuota item)
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

    public void RequireValid(DefaultReturnPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReturnPlanQuota> items)
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

/// <summary>Functions of default return plan quota rules.</summary>
public static class DefaultReturnPlanQuotaRules
{
    public static DefaultReturnPlanQuotaValidator DefaultDefaultReturnPlanQuotaValidator()
    {
        return new DefaultReturnPlanQuotaValidator(
            new List<IDefaultReturnPlanQuotaRule>
            {
                new DefaultReturnPlanQuotaCodeLengthRule(1000),
                new DefaultReturnPlanQuotaAllowedCodeRule(new List<string>()),
                new DefaultReturnPlanQuotaRequiredCodeRule("default return plan quota needs an entry"),
            });
    }
}
