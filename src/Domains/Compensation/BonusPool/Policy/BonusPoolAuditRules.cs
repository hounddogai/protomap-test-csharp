namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>A rule a bonus pool audit must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolAuditRule
{
    List<string> Check(BonusPoolAudit item);
}

public sealed record BonusPoolAuditCreatedAtRangeRule(int Min, int Max) : IBonusPoolAuditRule
{
    public List<string> Check(BonusPoolAudit item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolAuditDurationMinutesRangeRule(int Min, int Max) : IBonusPoolAuditRule
{
    public List<string> Check(BonusPoolAudit item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAuditArchivedGuardRule(int Limit) : IBonusPoolAuditRule
{
    public List<string> Check(BonusPoolAudit item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAuditNameLengthRule(int MaxLength) : IBonusPoolAuditRule
{
    public List<string> Check(BonusPoolAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool audit.</summary>
public sealed record BonusPoolAuditValidator(List<IBonusPoolAuditRule> Rules)
{
    public List<string> Validate(BonusPoolAudit item)
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

    public void RequireValid(BonusPoolAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolAudit> items)
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

/// <summary>Functions of bonus pool audit rules.</summary>
public static class BonusPoolAuditRules
{
    public static BonusPoolAuditValidator DefaultBonusPoolAuditValidator()
    {
        return new BonusPoolAuditValidator(
            new List<IBonusPoolAuditRule>
            {
                new BonusPoolAuditCreatedAtRangeRule(255, 255),
                new BonusPoolAuditDurationMinutesRangeRule(120, 86400),
                new BonusPoolAuditArchivedGuardRule(32),
                new BonusPoolAuditNameLengthRule(32),
            });
    }
}
