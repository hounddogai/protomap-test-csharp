namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool baseline must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolBaselineRule
{
    List<string> Check(BonusPoolBaseline item);
}

public sealed record BonusPoolBaselineCeilingRangeRule(double Min, double Max) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBaselineCostRangeRule(double Min, double Max) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolBaselineAllowedChannelRule(List<string> Allowed) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBaselineCodePrefixRule(string Prefix) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBaselineCodeLengthRule(int MaxLength) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBaselineLeaveDaysRangeRule(int Min, int Max) : IBonusPoolBaselineRule
{
    public List<string> Check(BonusPoolBaseline item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a bonus pool baseline.</summary>
public sealed record BonusPoolBaselineValidator(List<IBonusPoolBaselineRule> Rules)
{
    public List<string> Validate(BonusPoolBaseline item)
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

    public void RequireValid(BonusPoolBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolBaseline> items)
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

/// <summary>Functions of bonus pool baseline rules.</summary>
public static class BonusPoolBaselineRules
{
    public static BonusPoolBaselineValidator DefaultBonusPoolBaselineValidator()
    {
        return new BonusPoolBaselineValidator(
            new List<IBonusPoolBaselineRule>
            {
                new BonusPoolBaselineCeilingRangeRule(5.8, 2965.4),
                new BonusPoolBaselineCostRangeRule(3.8, 4084.4),
                new BonusPoolBaselineAllowedChannelRule(new List<string>()),
                new BonusPoolBaselineCodePrefixRule("ACM"),
                new BonusPoolBaselineCodeLengthRule(255),
                new BonusPoolBaselineLeaveDaysRangeRule(16, 3600),
            });
    }
}
