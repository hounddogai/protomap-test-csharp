namespace Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>A rule a lenient position profile must pass. Check returns its problems, or none.</summary>
public interface ILenientPositionProfileRule
{
    List<string> Check(LenientPositionProfile item);
}

public sealed record LenientPositionProfileCostRangeRule(double Min, double Max) : ILenientPositionProfileRule
{
    public List<string> Check(LenientPositionProfile item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionProfileRequiredTitleRule(string Message) : ILenientPositionProfileRule
{
    public List<string> Check(LenientPositionProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionProfileTitleLengthRule(int MaxLength) : ILenientPositionProfileRule
{
    public List<string> Check(LenientPositionProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionProfileCodePrefixRule(string Prefix) : ILenientPositionProfileRule
{
    public List<string> Check(LenientPositionProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionProfileTagsLimitRule(int MaxCount, List<string> Banned) : ILenientPositionProfileRule
{
    public List<string> Check(LenientPositionProfile item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient position profile.</summary>
public sealed record LenientPositionProfileValidator(List<ILenientPositionProfileRule> Rules)
{
    public List<string> Validate(LenientPositionProfile item)
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

    public void RequireValid(LenientPositionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPositionProfile> items)
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

/// <summary>Functions of lenient position profile rules.</summary>
public static class LenientPositionProfileRules
{
    public static LenientPositionProfileValidator DefaultLenientPositionProfileValidator()
    {
        return new LenientPositionProfileValidator(
            new List<ILenientPositionProfileRule>
            {
                new LenientPositionProfileCostRangeRule(9.5, 2901.0),
                new LenientPositionProfileRequiredTitleRule("lenient position profile needs an entry"),
                new LenientPositionProfileTitleLengthRule(80),
                new LenientPositionProfileCodePrefixRule("OPS"),
                new LenientPositionProfileTagsLimitRule(86400, new List<string> { "seasonal", "global" }),
            });
    }
}
