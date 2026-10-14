namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient bonus pool digest must pass. Check returns its problems, or none.</summary>
public interface ILenientBonusPoolDigestRule
{
    List<string> Check(LenientBonusPoolDigest item);
}

public sealed record LenientBonusPoolDigestAllowedSegmentRule(List<string> Allowed) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDigestRequiredCodeRule(string Message) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDigestRequiredSegmentRule(string Message) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDigestTitleLengthRule(int MaxLength) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDigestRequiredTitleRule(string Message) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolDigestCodePrefixRule(string Prefix) : ILenientBonusPoolDigestRule
{
    public List<string> Check(LenientBonusPoolDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient bonus pool digest.</summary>
public sealed record LenientBonusPoolDigestValidator(List<ILenientBonusPoolDigestRule> Rules)
{
    public List<string> Validate(LenientBonusPoolDigest item)
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

    public void RequireValid(LenientBonusPoolDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBonusPoolDigest> items)
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

/// <summary>Functions of lenient bonus pool digest rules.</summary>
public static class LenientBonusPoolDigestRules
{
    public static LenientBonusPoolDigestValidator DefaultLenientBonusPoolDigestValidator()
    {
        return new LenientBonusPoolDigestValidator(
            new List<ILenientBonusPoolDigestRule>
            {
                new LenientBonusPoolDigestAllowedSegmentRule(new List<string> { "primary" }),
                new LenientBonusPoolDigestRequiredCodeRule("lenient bonus pool digest needs this field"),
                new LenientBonusPoolDigestRequiredSegmentRule("lenient bonus pool digest needs an entry"),
                new LenientBonusPoolDigestTitleLengthRule(32),
                new LenientBonusPoolDigestRequiredTitleRule("lenient bonus pool digest needs an entry"),
                new LenientBonusPoolDigestCodePrefixRule("OPS"),
            });
    }
}
