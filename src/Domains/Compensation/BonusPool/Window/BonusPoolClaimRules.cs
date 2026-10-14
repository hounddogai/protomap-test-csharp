namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>A rule a bonus pool claim must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolClaimRule
{
    List<string> Check(BonusPoolClaim item);
}

public sealed record BonusPoolClaimMaximumRangeRule(double Min, double Max) : IBonusPoolClaimRule
{
    public List<string> Check(BonusPoolClaim item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolClaimCodeLengthRule(int MaxLength) : IBonusPoolClaimRule
{
    public List<string> Check(BonusPoolClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolClaimSkillLengthRule(int MaxLength) : IBonusPoolClaimRule
{
    public List<string> Check(BonusPoolClaim item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolClaimExpiresAtRangeRule(int Min, int Max) : IBonusPoolClaimRule
{
    public List<string> Check(BonusPoolClaim item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolClaimRequiredCodeRule(string Message) : IBonusPoolClaimRule
{
    public List<string> Check(BonusPoolClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool claim.</summary>
public sealed record BonusPoolClaimValidator(List<IBonusPoolClaimRule> Rules)
{
    public List<string> Validate(BonusPoolClaim item)
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

    public void RequireValid(BonusPoolClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolClaim> items)
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

/// <summary>Functions of bonus pool claim rules.</summary>
public static class BonusPoolClaimRules
{
    public static BonusPoolClaimValidator DefaultBonusPoolClaimValidator()
    {
        return new BonusPoolClaimValidator(
            new List<IBonusPoolClaimRule>
            {
                new BonusPoolClaimMaximumRangeRule(8.8, 609.8),
                new BonusPoolClaimCodeLengthRule(3600),
                new BonusPoolClaimSkillLengthRule(3600),
                new BonusPoolClaimExpiresAtRangeRule(16, 80),
                new BonusPoolClaimRequiredCodeRule("bonus pool claim needs an entry"),
            });
    }
}
