namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy policy ack claim must pass. Check returns its problems, or none.</summary>
public interface ILegacyPolicyAckClaimRule
{
    List<string> Check(LegacyPolicyAckClaim item);
}

public sealed record LegacyPolicyAckClaimAttemptsRangeRule(int Min, int Max) : ILegacyPolicyAckClaimRule
{
    public List<string> Check(LegacyPolicyAckClaim item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPolicyAckClaimSalaryBandLengthRule(int MaxLength) : ILegacyPolicyAckClaimRule
{
    public List<string> Check(LegacyPolicyAckClaim item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPolicyAckClaimAllowedTitleRule(List<string> Allowed) : ILegacyPolicyAckClaimRule
{
    public List<string> Check(LegacyPolicyAckClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy policy ack claim.</summary>
public sealed record LegacyPolicyAckClaimValidator(List<ILegacyPolicyAckClaimRule> Rules)
{
    public List<string> Validate(LegacyPolicyAckClaim item)
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

    public void RequireValid(LegacyPolicyAckClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPolicyAckClaim> items)
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

/// <summary>Functions of legacy policy ack claim rules.</summary>
public static class LegacyPolicyAckClaimRules
{
    public static LegacyPolicyAckClaimValidator DefaultLegacyPolicyAckClaimValidator()
    {
        return new LegacyPolicyAckClaimValidator(
            new List<ILegacyPolicyAckClaimRule>
            {
                new LegacyPolicyAckClaimAttemptsRangeRule(16, 16),
                new LegacyPolicyAckClaimSalaryBandLengthRule(80),
                new LegacyPolicyAckClaimAllowedTitleRule(new List<string> { "seasonal", "capped", "lenient" }),
            });
    }
}
