namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>A rule a legacy pay run audit must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayRunAuditRule
{
    List<string> Check(LegacyPayRunAudit item);
}

public sealed record LegacyPayRunAuditWindowDaysRangeRule(int Min, int Max) : ILegacyPayRunAuditRule
{
    public List<string> Check(LegacyPayRunAudit item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPayRunAuditCodePrefixRule(string Prefix) : ILegacyPayRunAuditRule
{
    public List<string> Check(LegacyPayRunAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAuditCapacityRangeRule(int Min, int Max) : ILegacyPayRunAuditRule
{
    public List<string> Check(LegacyPayRunAudit item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPayRunAuditCodeLengthRule(int MaxLength) : ILegacyPayRunAuditRule
{
    public List<string> Check(LegacyPayRunAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAuditRequiredSkillRule(string Message) : ILegacyPayRunAuditRule
{
    public List<string> Check(LegacyPayRunAudit item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay run audit.</summary>
public sealed record LegacyPayRunAuditValidator(List<ILegacyPayRunAuditRule> Rules)
{
    public List<string> Validate(LegacyPayRunAudit item)
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

    public void RequireValid(LegacyPayRunAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayRunAudit> items)
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

/// <summary>Functions of legacy pay run audit rules.</summary>
public static class LegacyPayRunAuditRules
{
    public static LegacyPayRunAuditValidator DefaultLegacyPayRunAuditValidator()
    {
        return new LegacyPayRunAuditValidator(
            new List<ILegacyPayRunAuditRule>
            {
                new LegacyPayRunAuditWindowDaysRangeRule(1000, 3600),
                new LegacyPayRunAuditCodePrefixRule("OPS"),
                new LegacyPayRunAuditCapacityRangeRule(64, 1000),
                new LegacyPayRunAuditCodeLengthRule(120),
                new LegacyPayRunAuditRequiredSkillRule("legacy pay run audit needs this field"),
            });
    }
}
