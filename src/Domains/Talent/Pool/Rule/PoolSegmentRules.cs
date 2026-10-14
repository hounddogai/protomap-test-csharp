namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool segment must pass. Check returns its problems, or none.</summary>
public interface IPoolSegmentRule
{
    List<string> Check(PoolSegment item);
}

public sealed record PoolSegmentAllowedTitleRule(List<string> Allowed) : IPoolSegmentRule
{
    public List<string> Check(PoolSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PoolSegmentWindowDaysRangeRule(int Min, int Max) : IPoolSegmentRule
{
    public List<string> Check(PoolSegment item)
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

public sealed record PoolSegmentCodePrefixRule(string Prefix) : IPoolSegmentRule
{
    public List<string> Check(PoolSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolSegmentRequiredManagerIdRule(string Message) : IPoolSegmentRule
{
    public List<string> Check(PoolSegment item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolSegmentCodeLengthRule(int MaxLength) : IPoolSegmentRule
{
    public List<string> Check(PoolSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool segment.</summary>
public sealed record PoolSegmentValidator(List<IPoolSegmentRule> Rules)
{
    public List<string> Validate(PoolSegment item)
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

    public void RequireValid(PoolSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolSegment> items)
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

/// <summary>Functions of pool segment rules.</summary>
public static class PoolSegmentRules
{
    public static PoolSegmentValidator DefaultPoolSegmentValidator()
    {
        return new PoolSegmentValidator(
            new List<IPoolSegmentRule>
            {
                new PoolSegmentAllowedTitleRule(new List<string> { "primary", "primary", "seasonal" }),
                new PoolSegmentWindowDaysRangeRule(32, 255),
                new PoolSegmentCodePrefixRule("X"),
                new PoolSegmentRequiredManagerIdRule("pool segment needs a value"),
                new PoolSegmentCodeLengthRule(255),
            });
    }
}
