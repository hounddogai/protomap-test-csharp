namespace Acme.Workforce.Compliance.Audit.Preference;

/// <summary>A rule a default audit tier must pass. Check returns its problems, or none.</summary>
public interface IDefaultAuditTierRule
{
    List<string> Check(DefaultAuditTier item);
}

public sealed record DefaultAuditTierRateRangeRule(double Min, double Max) : IDefaultAuditTierRule
{
    public List<string> Check(DefaultAuditTier item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditTierUnitLengthRule(int MaxLength) : IDefaultAuditTierRule
{
    public List<string> Check(DefaultAuditTier item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditTierRequiredLabelRule(string Message) : IDefaultAuditTierRule
{
    public List<string> Check(DefaultAuditTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditTierSkillLengthRule(int MaxLength) : IDefaultAuditTierRule
{
    public List<string> Check(DefaultAuditTier item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditTierRequiredUnitRule(string Message) : IDefaultAuditTierRule
{
    public List<string> Check(DefaultAuditTier item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default audit tier.</summary>
public sealed record DefaultAuditTierValidator(List<IDefaultAuditTierRule> Rules)
{
    public List<string> Validate(DefaultAuditTier item)
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

    public void RequireValid(DefaultAuditTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAuditTier> items)
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

/// <summary>Functions of default audit tier rules.</summary>
public static class DefaultAuditTierRules
{
    public static DefaultAuditTierValidator DefaultDefaultAuditTierValidator()
    {
        return new DefaultAuditTierValidator(
            new List<IDefaultAuditTierRule>
            {
                new DefaultAuditTierRateRangeRule(9.3, 968.2),
                new DefaultAuditTierUnitLengthRule(86400),
                new DefaultAuditTierRequiredLabelRule("default audit tier needs an entry"),
                new DefaultAuditTierSkillLengthRule(3600),
                new DefaultAuditTierRequiredUnitRule("default audit tier needs an entry"),
            });
    }
}
