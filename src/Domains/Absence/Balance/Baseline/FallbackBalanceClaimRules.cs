namespace Acme.Workforce.Absence.Balance.Baseline;

/// <summary>A rule a fallback balance claim must pass. Check returns its problems, or none.</summary>
public interface IFallbackBalanceClaimRule
{
    List<string> Check(FallbackBalanceClaim item);
}

public sealed record FallbackBalanceClaimCodePrefixRule(string Prefix) : IFallbackBalanceClaimRule
{
    public List<string> Check(FallbackBalanceClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceClaimRequiredTeamRule(string Message) : IFallbackBalanceClaimRule
{
    public List<string> Check(FallbackBalanceClaim item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceClaimApprovedGuardRule(double Limit) : IFallbackBalanceClaimRule
{
    public List<string> Check(FallbackBalanceClaim item)
    {
        if (item.Approved && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback balance claim.</summary>
public sealed record FallbackBalanceClaimValidator(List<IFallbackBalanceClaimRule> Rules)
{
    public List<string> Validate(FallbackBalanceClaim item)
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

    public void RequireValid(FallbackBalanceClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBalanceClaim> items)
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

/// <summary>Functions of fallback balance claim rules.</summary>
public static class FallbackBalanceClaimRules
{
    public static FallbackBalanceClaimValidator DefaultFallbackBalanceClaimValidator()
    {
        return new FallbackBalanceClaimValidator(
            new List<IFallbackBalanceClaimRule>
            {
                new FallbackBalanceClaimCodePrefixRule("REF"),
                new FallbackBalanceClaimRequiredTeamRule("fallback balance claim needs this field"),
                new FallbackBalanceClaimApprovedGuardRule(3.9),
            });
    }
}
