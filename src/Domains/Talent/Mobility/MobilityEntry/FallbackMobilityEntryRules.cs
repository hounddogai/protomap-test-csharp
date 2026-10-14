namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback mobility entry must pass. Check returns its problems, or none.</summary>
public interface IFallbackMobilityEntryRule
{
    List<string> Check(FallbackMobilityEntry item);
}

public sealed record FallbackMobilityEntryTitleLengthRule(int MaxLength) : IFallbackMobilityEntryRule
{
    public List<string> Check(FallbackMobilityEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityEntryFloorValueRangeRule(double Min, double Max) : IFallbackMobilityEntryRule
{
    public List<string> Check(FallbackMobilityEntry item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityEntryRequiredReviewerRule(string Message) : IFallbackMobilityEntryRule
{
    public List<string> Check(FallbackMobilityEntry item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityEntryAllowedTitleRule(List<string> Allowed) : IFallbackMobilityEntryRule
{
    public List<string> Check(FallbackMobilityEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMobilityEntryVersionRangeRule(int Min, int Max) : IFallbackMobilityEntryRule
{
    public List<string> Check(FallbackMobilityEntry item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback mobility entry.</summary>
public sealed record FallbackMobilityEntryValidator(List<IFallbackMobilityEntryRule> Rules)
{
    public List<string> Validate(FallbackMobilityEntry item)
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

    public void RequireValid(FallbackMobilityEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackMobilityEntry> items)
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

/// <summary>Functions of fallback mobility entry rules.</summary>
public static class FallbackMobilityEntryRules
{
    public static FallbackMobilityEntryValidator DefaultFallbackMobilityEntryValidator()
    {
        return new FallbackMobilityEntryValidator(
            new List<IFallbackMobilityEntryRule>
            {
                new FallbackMobilityEntryTitleLengthRule(86400),
                new FallbackMobilityEntryFloorValueRangeRule(9.8, 1778.2),
                new FallbackMobilityEntryRequiredReviewerRule("fallback mobility entry needs this field"),
                new FallbackMobilityEntryAllowedTitleRule(new List<string> { "legacy", "weighted", "strict" }),
                new FallbackMobilityEntryVersionRangeRule(64, 255),
            });
    }
}
