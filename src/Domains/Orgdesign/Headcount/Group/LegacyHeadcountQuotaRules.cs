namespace Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>A rule a legacy headcount quota must pass. Check returns its problems, or none.</summary>
public interface ILegacyHeadcountQuotaRule
{
    List<string> Check(LegacyHeadcountQuota item);
}

public sealed record LegacyHeadcountQuotaDepartmentLengthRule(int MaxLength) : ILegacyHeadcountQuotaRule
{
    public List<string> Check(LegacyHeadcountQuota item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountQuotaCodeLengthRule(int MaxLength) : ILegacyHeadcountQuotaRule
{
    public List<string> Check(LegacyHeadcountQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountQuotaRequiredDepartmentRule(string Message) : ILegacyHeadcountQuotaRule
{
    public List<string> Check(LegacyHeadcountQuota item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy headcount quota.</summary>
public sealed record LegacyHeadcountQuotaValidator(List<ILegacyHeadcountQuotaRule> Rules)
{
    public List<string> Validate(LegacyHeadcountQuota item)
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

    public void RequireValid(LegacyHeadcountQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyHeadcountQuota> items)
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

/// <summary>Functions of legacy headcount quota rules.</summary>
public static class LegacyHeadcountQuotaRules
{
    public static LegacyHeadcountQuotaValidator DefaultLegacyHeadcountQuotaValidator()
    {
        return new LegacyHeadcountQuotaValidator(
            new List<ILegacyHeadcountQuotaRule>
            {
                new LegacyHeadcountQuotaDepartmentLengthRule(120),
                new LegacyHeadcountQuotaCodeLengthRule(64),
                new LegacyHeadcountQuotaRequiredDepartmentRule("legacy headcount quota needs a value"),
            });
    }
}
