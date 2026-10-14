namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling access badge profile must pass. Check returns its problems, or none.</summary>
public interface IRollingAccessBadgeProfileRule
{
    List<string> Check(RollingAccessBadgeProfile item);
}

public sealed record RollingAccessBadgeProfileBadgeLengthRule(int MaxLength) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeProfileCodePrefixRule(string Prefix) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeProfileCapacityRangeRule(int Min, int Max) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccessBadgeProfileAllowedPhoneRule(List<string> Allowed) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeProfileWindowDaysRangeRule(int Min, int Max) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccessBadgeProfileRequiredCodeRule(string Message) : IRollingAccessBadgeProfileRule
{
    public List<string> Check(RollingAccessBadgeProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling access badge profile.</summary>
public sealed record RollingAccessBadgeProfileValidator(List<IRollingAccessBadgeProfileRule> Rules)
{
    public List<string> Validate(RollingAccessBadgeProfile item)
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

    public void RequireValid(RollingAccessBadgeProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccessBadgeProfile> items)
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

/// <summary>Functions of rolling access badge profile rules.</summary>
public static class RollingAccessBadgeProfileRules
{
    public static RollingAccessBadgeProfileValidator DefaultRollingAccessBadgeProfileValidator()
    {
        return new RollingAccessBadgeProfileValidator(
            new List<IRollingAccessBadgeProfileRule>
            {
                new RollingAccessBadgeProfileBadgeLengthRule(80),
                new RollingAccessBadgeProfileCodePrefixRule("Q"),
                new RollingAccessBadgeProfileCapacityRangeRule(255, 86400),
                new RollingAccessBadgeProfileAllowedPhoneRule(new List<string> { "batched" }),
                new RollingAccessBadgeProfileWindowDaysRangeRule(80, 3600),
                new RollingAccessBadgeProfileRequiredCodeRule("rolling access badge profile needs an entry"),
            });
    }
}
