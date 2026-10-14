namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback succession plan quota must pass. Check returns its problems, or none.</summary>
public interface IFallbackSuccessionPlanQuotaRule
{
    List<string> Check(FallbackSuccessionPlanQuota item);
}

public sealed record FallbackSuccessionPlanQuotaNameLengthRule(int MaxLength) : IFallbackSuccessionPlanQuotaRule
{
    public List<string> Check(FallbackSuccessionPlanQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanQuotaAllowedCodeRule(List<string> Allowed) : IFallbackSuccessionPlanQuotaRule
{
    public List<string> Check(FallbackSuccessionPlanQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanQuotaNotesLengthRule(int MaxLength) : IFallbackSuccessionPlanQuotaRule
{
    public List<string> Check(FallbackSuccessionPlanQuota item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSuccessionPlanQuotaRequiredNameRule(string Message) : IFallbackSuccessionPlanQuotaRule
{
    public List<string> Check(FallbackSuccessionPlanQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback succession plan quota.</summary>
public sealed record FallbackSuccessionPlanQuotaValidator(List<IFallbackSuccessionPlanQuotaRule> Rules)
{
    public List<string> Validate(FallbackSuccessionPlanQuota item)
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

    public void RequireValid(FallbackSuccessionPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSuccessionPlanQuota> items)
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

/// <summary>Functions of fallback succession plan quota rules.</summary>
public static class FallbackSuccessionPlanQuotaRules
{
    public static FallbackSuccessionPlanQuotaValidator DefaultFallbackSuccessionPlanQuotaValidator()
    {
        return new FallbackSuccessionPlanQuotaValidator(
            new List<IFallbackSuccessionPlanQuotaRule>
            {
                new FallbackSuccessionPlanQuotaNameLengthRule(1000),
                new FallbackSuccessionPlanQuotaAllowedCodeRule(new List<string> { "weighted", "global", "legacy" }),
                new FallbackSuccessionPlanQuotaNotesLengthRule(3600),
                new FallbackSuccessionPlanQuotaRequiredNameRule("fallback succession plan quota needs an entry"),
            });
    }
}
