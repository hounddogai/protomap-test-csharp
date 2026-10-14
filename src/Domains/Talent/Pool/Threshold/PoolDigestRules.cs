namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool digest must pass. Check returns its problems, or none.</summary>
public interface IPoolDigestRule
{
    List<string> Check(PoolDigest item);
}

public sealed record PoolDigestTitleLengthRule(int MaxLength) : IPoolDigestRule
{
    public List<string> Check(PoolDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolDigestAllowedLocationRule(List<string> Allowed) : IPoolDigestRule
{
    public List<string> Check(PoolDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PoolDigestSequenceRangeRule(int Min, int Max) : IPoolDigestRule
{
    public List<string> Check(PoolDigest item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pool digest.</summary>
public sealed record PoolDigestValidator(List<IPoolDigestRule> Rules)
{
    public List<string> Validate(PoolDigest item)
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

    public void RequireValid(PoolDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolDigest> items)
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

/// <summary>Functions of pool digest rules.</summary>
public static class PoolDigestRules
{
    public static PoolDigestValidator DefaultPoolDigestValidator()
    {
        return new PoolDigestValidator(
            new List<IPoolDigestRule>
            {
                new PoolDigestTitleLengthRule(255),
                new PoolDigestAllowedLocationRule(new List<string> { "manual", "batched" }),
                new PoolDigestSequenceRangeRule(16, 32),
            });
    }
}
