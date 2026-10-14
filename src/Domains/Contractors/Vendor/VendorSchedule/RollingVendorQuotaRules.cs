namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling vendor quota must pass. Check returns its problems, or none.</summary>
public interface IRollingVendorQuotaRule
{
    List<string> Check(RollingVendorQuota item);
}

public sealed record RollingVendorQuotaCodeLengthRule(int MaxLength) : IRollingVendorQuotaRule
{
    public List<string> Check(RollingVendorQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingVendorQuotaAllowedLabelRule(List<string> Allowed) : IRollingVendorQuotaRule
{
    public List<string> Check(RollingVendorQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RollingVendorQuotaRequiredLabelRule(string Message) : IRollingVendorQuotaRule
{
    public List<string> Check(RollingVendorQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling vendor quota.</summary>
public sealed record RollingVendorQuotaValidator(List<IRollingVendorQuotaRule> Rules)
{
    public List<string> Validate(RollingVendorQuota item)
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

    public void RequireValid(RollingVendorQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingVendorQuota> items)
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

/// <summary>Functions of rolling vendor quota rules.</summary>
public static class RollingVendorQuotaRules
{
    public static RollingVendorQuotaValidator DefaultRollingVendorQuotaValidator()
    {
        return new RollingVendorQuotaValidator(
            new List<IRollingVendorQuotaRule>
            {
                new RollingVendorQuotaCodeLengthRule(32),
                new RollingVendorQuotaAllowedLabelRule(new List<string> { "capped", "strict" }),
                new RollingVendorQuotaRequiredLabelRule("rolling vendor quota needs an entry"),
            });
    }
}
