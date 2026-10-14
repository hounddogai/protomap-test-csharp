namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pool rule must pass. Check returns its problems, or none.</summary>
public interface IScheduledPoolRuleRule
{
    List<string> Check(ScheduledPoolRule item);
}

public sealed record ScheduledPoolRuleAllowedTitleRule(List<string> Allowed) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolRuleMinimumRangeRule(double Min, double Max) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPoolRuleCapacityRangeRule(int Min, int Max) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolRuleCodePrefixRule(string Prefix) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolRuleVisibleGuardRule(double Limit) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        if (item.Visible && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolRuleDurationMinutesRangeRule(int Min, int Max) : IScheduledPoolRuleRule
{
    public List<string> Check(ScheduledPoolRule item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled pool rule.</summary>
public sealed record ScheduledPoolRuleValidator(List<IScheduledPoolRuleRule> Rules)
{
    public List<string> Validate(ScheduledPoolRule item)
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

    public void RequireValid(ScheduledPoolRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPoolRule> items)
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

/// <summary>Functions of scheduled pool rule rules.</summary>
public static class ScheduledPoolRuleRules
{
    public static ScheduledPoolRuleValidator DefaultScheduledPoolRuleValidator()
    {
        return new ScheduledPoolRuleValidator(
            new List<IScheduledPoolRuleRule>
            {
                new ScheduledPoolRuleAllowedTitleRule(new List<string> { "regional", "global", "fallback" }),
                new ScheduledPoolRuleMinimumRangeRule(3.0, 2315.4),
                new ScheduledPoolRuleCapacityRangeRule(120, 255),
                new ScheduledPoolRuleCodePrefixRule("REF"),
                new ScheduledPoolRuleVisibleGuardRule(2.3),
                new ScheduledPoolRuleDurationMinutesRangeRule(120, 86400),
            });
    }
}
