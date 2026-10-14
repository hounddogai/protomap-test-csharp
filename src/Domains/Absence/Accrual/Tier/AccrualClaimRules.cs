namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual claim must pass. Check returns its problems, or none.</summary>
public interface IAccrualClaimRule
{
    List<string> Check(AccrualClaim item);
}

public sealed record AccrualClaimAllowedDepartmentRule(List<string> Allowed) : IAccrualClaimRule
{
    public List<string> Check(AccrualClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AccrualClaimDepartmentLengthRule(int MaxLength) : IAccrualClaimRule
{
    public List<string> Check(AccrualClaim item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualClaimCodePrefixRule(string Prefix) : IAccrualClaimRule
{
    public List<string> Check(AccrualClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual claim.</summary>
public sealed record AccrualClaimValidator(List<IAccrualClaimRule> Rules)
{
    public List<string> Validate(AccrualClaim item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(AccrualClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualClaim> items)
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

/// <summary>Functions of accrual claim rules.</summary>
public static class AccrualClaimRules
{
    public static AccrualClaimValidator DefaultAccrualClaimValidator()
    {
        return new AccrualClaimValidator(
            new List<IAccrualClaimRule>
            {
                new AccrualClaimAllowedDepartmentRule(new List<string> { "global", "global" }),
                new AccrualClaimDepartmentLengthRule(120),
                new AccrualClaimCodePrefixRule("X"),
            });
    }
}
