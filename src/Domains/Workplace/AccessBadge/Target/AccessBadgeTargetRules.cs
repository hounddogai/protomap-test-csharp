namespace Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>A rule a access badge target must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeTargetRule
{
    List<string> Check(AccessBadgeTarget item);
}

public sealed record AccessBadgeTargetRequiredRegionRule(string Message) : IAccessBadgeTargetRule
{
    public List<string> Check(AccessBadgeTarget item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTargetWeightRangeRule(double Min, double Max) : IAccessBadgeTargetRule
{
    public List<string> Check(AccessBadgeTarget item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTargetNameLengthRule(int MaxLength) : IAccessBadgeTargetRule
{
    public List<string> Check(AccessBadgeTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTargetRegionLengthRule(int MaxLength) : IAccessBadgeTargetRule
{
    public List<string> Check(AccessBadgeTarget item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge target.</summary>
public sealed record AccessBadgeTargetValidator(List<IAccessBadgeTargetRule> Rules)
{
    public List<string> Validate(AccessBadgeTarget item)
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

    public void RequireValid(AccessBadgeTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeTarget> items)
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

/// <summary>Functions of access badge target rules.</summary>
public static class AccessBadgeTargetRules
{
    public static AccessBadgeTargetValidator DefaultAccessBadgeTargetValidator()
    {
        return new AccessBadgeTargetValidator(
            new List<IAccessBadgeTargetRule>
            {
                new AccessBadgeTargetRequiredRegionRule("access badge target needs this field"),
                new AccessBadgeTargetWeightRangeRule(8.8, 1654.4),
                new AccessBadgeTargetNameLengthRule(255),
                new AccessBadgeTargetRegionLengthRule(80),
            });
    }
}
