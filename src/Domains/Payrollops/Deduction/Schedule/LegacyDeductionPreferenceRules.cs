namespace Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>A rule a legacy deduction preference must pass. Check returns its problems, or none.</summary>
public interface ILegacyDeductionPreferenceRule
{
    List<string> Check(LegacyDeductionPreference item);
}

public sealed record LegacyDeductionPreferencePinnedGuardRule(double Limit) : ILegacyDeductionPreferenceRule
{
    public List<string> Check(LegacyDeductionPreference item)
    {
        if (item.Pinned && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionPreferenceRequiredBadgeRule(string Message) : ILegacyDeductionPreferenceRule
{
    public List<string> Check(LegacyDeductionPreference item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionPreferenceCodePrefixRule(string Prefix) : ILegacyDeductionPreferenceRule
{
    public List<string> Check(LegacyDeductionPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionPreferenceRequiredCategoryRule(string Message) : ILegacyDeductionPreferenceRule
{
    public List<string> Check(LegacyDeductionPreference item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeductionPreferenceBadgeLengthRule(int MaxLength) : ILegacyDeductionPreferenceRule
{
    public List<string> Check(LegacyDeductionPreference item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy deduction preference.</summary>
public sealed record LegacyDeductionPreferenceValidator(List<ILegacyDeductionPreferenceRule> Rules)
{
    public List<string> Validate(LegacyDeductionPreference item)
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

    public void RequireValid(LegacyDeductionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyDeductionPreference> items)
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

/// <summary>Functions of legacy deduction preference rules.</summary>
public static class LegacyDeductionPreferenceRules
{
    public static LegacyDeductionPreferenceValidator DefaultLegacyDeductionPreferenceValidator()
    {
        return new LegacyDeductionPreferenceValidator(
            new List<ILegacyDeductionPreferenceRule>
            {
                new LegacyDeductionPreferencePinnedGuardRule(1.6),
                new LegacyDeductionPreferenceRequiredBadgeRule("legacy deduction preference needs this field"),
                new LegacyDeductionPreferenceCodePrefixRule("X"),
                new LegacyDeductionPreferenceRequiredCategoryRule("legacy deduction preference needs this field"),
                new LegacyDeductionPreferenceBadgeLengthRule(32),
            });
    }
}
