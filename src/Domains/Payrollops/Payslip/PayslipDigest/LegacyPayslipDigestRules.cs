namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy payslip digest must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayslipDigestRule
{
    List<string> Check(LegacyPayslipDigest item);
}

public sealed record LegacyPayslipDigestRequiredTitleRule(string Message) : ILegacyPayslipDigestRule
{
    public List<string> Check(LegacyPayslipDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayslipDigestRequiredOwnerIdRule(string Message) : ILegacyPayslipDigestRule
{
    public List<string> Check(LegacyPayslipDigest item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayslipDigestAllowedCodeRule(List<string> Allowed) : ILegacyPayslipDigestRule
{
    public List<string> Check(LegacyPayslipDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayslipDigestGraceDaysRangeRule(int Min, int Max) : ILegacyPayslipDigestRule
{
    public List<string> Check(LegacyPayslipDigest item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPayslipDigestOwnerIdLengthRule(int MaxLength) : ILegacyPayslipDigestRule
{
    public List<string> Check(LegacyPayslipDigest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy payslip digest.</summary>
public sealed record LegacyPayslipDigestValidator(List<ILegacyPayslipDigestRule> Rules)
{
    public List<string> Validate(LegacyPayslipDigest item)
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

    public void RequireValid(LegacyPayslipDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayslipDigest> items)
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

/// <summary>Functions of legacy payslip digest rules.</summary>
public static class LegacyPayslipDigestRules
{
    public static LegacyPayslipDigestValidator DefaultLegacyPayslipDigestValidator()
    {
        return new LegacyPayslipDigestValidator(
            new List<ILegacyPayslipDigestRule>
            {
                new LegacyPayslipDigestRequiredTitleRule("legacy payslip digest needs an entry"),
                new LegacyPayslipDigestRequiredOwnerIdRule("legacy payslip digest needs an entry"),
                new LegacyPayslipDigestAllowedCodeRule(new List<string> { "strict", "batched" }),
                new LegacyPayslipDigestGraceDaysRangeRule(64, 120),
                new LegacyPayslipDigestOwnerIdLengthRule(32),
            });
    }
}
