namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict policy ack claim must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyAckClaimRule
{
    List<string> Check(StrictPolicyAckClaim item);
}

public sealed record StrictPolicyAckClaimSkillLengthRule(int MaxLength) : IStrictPolicyAckClaimRule
{
    public List<string> Check(StrictPolicyAckClaim item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckClaimAllowedSummaryRule(List<string> Allowed) : IStrictPolicyAckClaimRule
{
    public List<string> Check(StrictPolicyAckClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckClaimCodeLengthRule(int MaxLength) : IStrictPolicyAckClaimRule
{
    public List<string> Check(StrictPolicyAckClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict policy ack claim.</summary>
public sealed record StrictPolicyAckClaimValidator(List<IStrictPolicyAckClaimRule> Rules)
{
    public List<string> Validate(StrictPolicyAckClaim item)
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

    public void RequireValid(StrictPolicyAckClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyAckClaim> items)
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

/// <summary>Functions of strict policy ack claim rules.</summary>
public static class StrictPolicyAckClaimRules
{
    public static StrictPolicyAckClaimValidator DefaultStrictPolicyAckClaimValidator()
    {
        return new StrictPolicyAckClaimValidator(
            new List<IStrictPolicyAckClaimRule>
            {
                new StrictPolicyAckClaimSkillLengthRule(3600),
                new StrictPolicyAckClaimAllowedSummaryRule(new List<string> { "tiered" }),
                new StrictPolicyAckClaimCodeLengthRule(32),
            });
    }
}
