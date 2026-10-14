namespace Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>A rule a legacy accrual claim must pass. Check returns its problems, or none.</summary>
public interface ILegacyAccrualClaimRule
{
    List<string> Check(LegacyAccrualClaim item);
}

public sealed record LegacyAccrualClaimDescriptionLengthRule(int MaxLength) : ILegacyAccrualClaimRule
{
    public List<string> Check(LegacyAccrualClaim item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccrualClaimRequiredCodeRule(string Message) : ILegacyAccrualClaimRule
{
    public List<string> Check(LegacyAccrualClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccrualClaimRequiredTitleRule(string Message) : ILegacyAccrualClaimRule
{
    public List<string> Check(LegacyAccrualClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy accrual claim.</summary>
public sealed record LegacyAccrualClaimValidator(List<ILegacyAccrualClaimRule> Rules)
{
    public List<string> Validate(LegacyAccrualClaim item)
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

    public void RequireValid(LegacyAccrualClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAccrualClaim> items)
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

/// <summary>Functions of legacy accrual claim rules.</summary>
public static class LegacyAccrualClaimRules
{
    public static LegacyAccrualClaimValidator DefaultLegacyAccrualClaimValidator()
    {
        return new LegacyAccrualClaimValidator(
            new List<ILegacyAccrualClaimRule>
            {
                new LegacyAccrualClaimDescriptionLengthRule(16),
                new LegacyAccrualClaimRequiredCodeRule("legacy accrual claim needs a value"),
                new LegacyAccrualClaimRequiredTitleRule("legacy accrual claim needs this field"),
            });
    }
}
