namespace Acme.Workforce.Assets.Assignment.Exception;

/// <summary>A rule a regional assignment tier must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssignmentTierRule
{
    List<string> Check(RegionalAssignmentTier item);
}

public sealed record RegionalAssignmentTierCodeLengthRule(int MaxLength) : IRegionalAssignmentTierRule
{
    public List<string> Check(RegionalAssignmentTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentTierSkillLengthRule(int MaxLength) : IRegionalAssignmentTierRule
{
    public List<string> Check(RegionalAssignmentTier item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentTierVisibleGuardRule(int Limit) : IRegionalAssignmentTierRule
{
    public List<string> Check(RegionalAssignmentTier item)
    {
        if (item.Visible && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentTierLimitRangeRule(int Min, int Max) : IRegionalAssignmentTierRule
{
    public List<string> Check(RegionalAssignmentTier item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentTierNameLengthRule(int MaxLength) : IRegionalAssignmentTierRule
{
    public List<string> Check(RegionalAssignmentTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional assignment tier.</summary>
public sealed record RegionalAssignmentTierValidator(List<IRegionalAssignmentTierRule> Rules)
{
    public List<string> Validate(RegionalAssignmentTier item)
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

    public void RequireValid(RegionalAssignmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssignmentTier> items)
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

/// <summary>Functions of regional assignment tier rules.</summary>
public static class RegionalAssignmentTierRules
{
    public static RegionalAssignmentTierValidator DefaultRegionalAssignmentTierValidator()
    {
        return new RegionalAssignmentTierValidator(
            new List<IRegionalAssignmentTierRule>
            {
                new RegionalAssignmentTierCodeLengthRule(64),
                new RegionalAssignmentTierSkillLengthRule(120),
                new RegionalAssignmentTierVisibleGuardRule(16),
                new RegionalAssignmentTierLimitRangeRule(80, 86400),
                new RegionalAssignmentTierNameLengthRule(255),
            });
    }
}
