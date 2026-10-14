namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy audit claim must pass. Check returns its problems, or none.</summary>
public interface ILegacyAuditClaimRule
{
    List<string> Check(LegacyAuditClaim item);
}

public sealed record LegacyAuditClaimArchivedGuardRule(int Limit) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        if (item.Archived && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditClaimRequiredOwnerIdRule(string Message) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditClaimRevisionRangeRule(int Min, int Max) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditClaimOwnerIdLengthRule(int MaxLength) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditClaimAllowedTitleRule(List<string> Allowed) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAuditClaimRequiredTitleRule(string Message) : ILegacyAuditClaimRule
{
    public List<string> Check(LegacyAuditClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy audit claim.</summary>
public sealed record LegacyAuditClaimValidator(List<ILegacyAuditClaimRule> Rules)
{
    public List<string> Validate(LegacyAuditClaim item)
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

    public void RequireValid(LegacyAuditClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAuditClaim> items)
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

/// <summary>Functions of legacy audit claim rules.</summary>
public static class LegacyAuditClaimRules
{
    public static LegacyAuditClaimValidator DefaultLegacyAuditClaimValidator()
    {
        return new LegacyAuditClaimValidator(
            new List<ILegacyAuditClaimRule>
            {
                new LegacyAuditClaimArchivedGuardRule(255),
                new LegacyAuditClaimRequiredOwnerIdRule("legacy audit claim needs an entry"),
                new LegacyAuditClaimRevisionRangeRule(64, 1000),
                new LegacyAuditClaimOwnerIdLengthRule(3600),
                new LegacyAuditClaimAllowedTitleRule(new List<string> { "capped", "fallback" }),
                new LegacyAuditClaimRequiredTitleRule("legacy audit claim needs an entry"),
            });
    }
}
