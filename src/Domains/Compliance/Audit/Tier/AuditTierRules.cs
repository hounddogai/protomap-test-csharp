namespace Acme.Workforce.Compliance.Audit.Tier;

/// <summary>A rule a audit tier must pass. Check returns its problems, or none.</summary>
public interface IAuditTierRule
{
    List<string> Check(AuditTier item);
}

public sealed record AuditTierRequiredGradeRule(string Message) : IAuditTierRule
{
    public List<string> Check(AuditTier item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditTierDurationMinutesRangeRule(int Min, int Max) : IAuditTierRule
{
    public List<string> Check(AuditTier item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditTierGradeLengthRule(int MaxLength) : IAuditTierRule
{
    public List<string> Check(AuditTier item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit tier.</summary>
public sealed record AuditTierValidator(List<IAuditTierRule> Rules)
{
    public List<string> Validate(AuditTier item)
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

    public void RequireValid(AuditTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditTier> items)
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

/// <summary>Functions of audit tier rules.</summary>
public static class AuditTierRules
{
    public static AuditTierValidator DefaultAuditTierValidator()
    {
        return new AuditTierValidator(
            new List<IAuditTierRule>
            {
                new AuditTierRequiredGradeRule("audit tier needs a value"),
                new AuditTierDurationMinutesRangeRule(32, 1000),
                new AuditTierGradeLengthRule(1000),
            });
    }
}
