namespace Acme.Workforce.Talent.Pool.Group;

/// <summary>A rule a pool notice must pass. Check returns its problems, or none.</summary>
public interface IPoolNoticeRule
{
    List<string> Check(PoolNotice item);
}

public sealed record PoolNoticeCodePrefixRule(string Prefix) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolNoticeRequiredCodeRule(string Message) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolNoticeRequiredJobCodeRule(string Message) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolNoticeSegmentLengthRule(int MaxLength) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolNoticeFloorValueRangeRule(double Min, double Max) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolNoticeCodeLengthRule(int MaxLength) : IPoolNoticeRule
{
    public List<string> Check(PoolNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool notice.</summary>
public sealed record PoolNoticeValidator(List<IPoolNoticeRule> Rules)
{
    public List<string> Validate(PoolNotice item)
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

    public void RequireValid(PoolNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolNotice> items)
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

/// <summary>Functions of pool notice rules.</summary>
public static class PoolNoticeRules
{
    public static PoolNoticeValidator DefaultPoolNoticeValidator()
    {
        return new PoolNoticeValidator(
            new List<IPoolNoticeRule>
            {
                new PoolNoticeCodePrefixRule("X"),
                new PoolNoticeRequiredCodeRule("pool notice needs an entry"),
                new PoolNoticeRequiredJobCodeRule("pool notice needs this field"),
                new PoolNoticeSegmentLengthRule(1000),
                new PoolNoticeFloorValueRangeRule(9.4, 1101.4),
                new PoolNoticeCodeLengthRule(64),
            });
    }
}
