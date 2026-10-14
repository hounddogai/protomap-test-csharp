namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered offboarding segment must pass. Check returns its problems, or none.</summary>
public interface ITieredOffboardingSegmentRule
{
    List<string> Check(TieredOffboardingSegment item);
}

public sealed record TieredOffboardingSegmentCodeLengthRule(int MaxLength) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingSegmentAllowedTitleRule(List<string> Allowed) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
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

public sealed record TieredOffboardingSegmentRequiredCodeRule(string Message) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingSegmentRequiredTitleRule(string Message) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingSegmentRequiredChecksumRule(string Message) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingSegmentChecksumLengthRule(int MaxLength) : ITieredOffboardingSegmentRule
{
    public List<string> Check(TieredOffboardingSegment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered offboarding segment.</summary>
public sealed record TieredOffboardingSegmentValidator(List<ITieredOffboardingSegmentRule> Rules)
{
    public List<string> Validate(TieredOffboardingSegment item)
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

    public void RequireValid(TieredOffboardingSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredOffboardingSegment> items)
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

/// <summary>Functions of tiered offboarding segment rules.</summary>
public static class TieredOffboardingSegmentRules
{
    public static TieredOffboardingSegmentValidator DefaultTieredOffboardingSegmentValidator()
    {
        return new TieredOffboardingSegmentValidator(
            new List<ITieredOffboardingSegmentRule>
            {
                new TieredOffboardingSegmentCodeLengthRule(1000),
                new TieredOffboardingSegmentAllowedTitleRule(new List<string>()),
                new TieredOffboardingSegmentRequiredCodeRule("tiered offboarding segment needs a value"),
                new TieredOffboardingSegmentRequiredTitleRule("tiered offboarding segment needs this field"),
                new TieredOffboardingSegmentRequiredChecksumRule("tiered offboarding segment needs a value"),
                new TieredOffboardingSegmentChecksumLengthRule(16),
            });
    }
}
