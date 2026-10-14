namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge milestone must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeMilestoneRule
{
    List<string> Check(AccessBadgeMilestone item);
}

public sealed record AccessBadgeMilestoneRateRangeRule(double Min, double Max) : IAccessBadgeMilestoneRule
{
    public List<string> Check(AccessBadgeMilestone item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMilestoneUnitLengthRule(int MaxLength) : IAccessBadgeMilestoneRule
{
    public List<string> Check(AccessBadgeMilestone item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMilestoneAllowedCurrencyRule(List<string> Allowed) : IAccessBadgeMilestoneRule
{
    public List<string> Check(AccessBadgeMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMilestoneCodePrefixRule(string Prefix) : IAccessBadgeMilestoneRule
{
    public List<string> Check(AccessBadgeMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMilestoneSkillLengthRule(int MaxLength) : IAccessBadgeMilestoneRule
{
    public List<string> Check(AccessBadgeMilestone item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge milestone.</summary>
public sealed record AccessBadgeMilestoneValidator(List<IAccessBadgeMilestoneRule> Rules)
{
    public List<string> Validate(AccessBadgeMilestone item)
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

    public void RequireValid(AccessBadgeMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeMilestone> items)
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

/// <summary>Functions of access badge milestone rules.</summary>
public static class AccessBadgeMilestoneRules
{
    public static AccessBadgeMilestoneValidator DefaultAccessBadgeMilestoneValidator()
    {
        return new AccessBadgeMilestoneValidator(
            new List<IAccessBadgeMilestoneRule>
            {
                new AccessBadgeMilestoneRateRangeRule(9.4, 2173.2),
                new AccessBadgeMilestoneUnitLengthRule(86400),
                new AccessBadgeMilestoneAllowedCurrencyRule(new List<string>()),
                new AccessBadgeMilestoneCodePrefixRule("Q"),
                new AccessBadgeMilestoneSkillLengthRule(86400),
            });
    }
}
