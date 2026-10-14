namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>A rule a access badge rollup must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeRollupRule
{
    List<string> Check(AccessBadgeRollup item);
}

public sealed record AccessBadgeRollupCapacityRangeRule(int Min, int Max) : IAccessBadgeRollupRule
{
    public List<string> Check(AccessBadgeRollup item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRollupPinnedGuardRule(double Limit) : IAccessBadgeRollupRule
{
    public List<string> Check(AccessBadgeRollup item)
    {
        if (item.Pinned && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRollupHoursRangeRule(double Min, double Max) : IAccessBadgeRollupRule
{
    public List<string> Check(AccessBadgeRollup item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeRollupPhoneLengthRule(int MaxLength) : IAccessBadgeRollupRule
{
    public List<string> Check(AccessBadgeRollup item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRollupRequiredCodeRule(string Message) : IAccessBadgeRollupRule
{
    public List<string> Check(AccessBadgeRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge rollup.</summary>
public sealed record AccessBadgeRollupValidator(List<IAccessBadgeRollupRule> Rules)
{
    public List<string> Validate(AccessBadgeRollup item)
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

    public void RequireValid(AccessBadgeRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeRollup> items)
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

/// <summary>Functions of access badge rollup rules.</summary>
public static class AccessBadgeRollupRules
{
    public static AccessBadgeRollupValidator DefaultAccessBadgeRollupValidator()
    {
        return new AccessBadgeRollupValidator(
            new List<IAccessBadgeRollupRule>
            {
                new AccessBadgeRollupCapacityRangeRule(16, 255),
                new AccessBadgeRollupPinnedGuardRule(3.2),
                new AccessBadgeRollupHoursRangeRule(7.9, 1547.3),
                new AccessBadgeRollupPhoneLengthRule(32),
                new AccessBadgeRollupRequiredCodeRule("access badge rollup needs this field"),
            });
    }
}
