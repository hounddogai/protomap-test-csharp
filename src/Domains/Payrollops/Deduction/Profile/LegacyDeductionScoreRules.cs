namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy deduction score must pass. Check returns its problems, or none.</summary>
public interface ILegacyDeductionScoreRule
{
    List<string> Check(LegacyDeductionScore item);
}

public sealed record LegacyDeductionScoreRequiredBadgeRule(string Message) : ILegacyDeductionScoreRule
{
    public List<string> Check(LegacyDeductionScore item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionScoreNameLengthRule(int MaxLength) : ILegacyDeductionScoreRule
{
    public List<string> Check(LegacyDeductionScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionScoreLimitRangeRule(int Min, int Max) : ILegacyDeductionScoreRule
{
    public List<string> Check(LegacyDeductionScore item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionScoreTenureMonthsRangeRule(int Min, int Max) : ILegacyDeductionScoreRule
{
    public List<string> Check(LegacyDeductionScore item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyDeductionScoreAllowedOwnerIdRule(List<string> Allowed) : ILegacyDeductionScoreRule
{
    public List<string> Check(LegacyDeductionScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy deduction score.</summary>
public sealed record LegacyDeductionScoreValidator(List<ILegacyDeductionScoreRule> Rules)
{
    public List<string> Validate(LegacyDeductionScore item)
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

    public void RequireValid(LegacyDeductionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyDeductionScore> items)
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

/// <summary>Functions of legacy deduction score rules.</summary>
public static class LegacyDeductionScoreRules
{
    public static LegacyDeductionScoreValidator DefaultLegacyDeductionScoreValidator()
    {
        return new LegacyDeductionScoreValidator(
            new List<ILegacyDeductionScoreRule>
            {
                new LegacyDeductionScoreRequiredBadgeRule("legacy deduction score needs an entry"),
                new LegacyDeductionScoreNameLengthRule(32),
                new LegacyDeductionScoreLimitRangeRule(80, 1000),
                new LegacyDeductionScoreTenureMonthsRangeRule(16, 64),
                new LegacyDeductionScoreAllowedOwnerIdRule(new List<string> { "tiered", "weighted", "regional" }),
            });
    }
}
