namespace Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>A rule a adaptive engagement entry must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveEngagementEntryRule
{
    List<string> Check(AdaptiveEngagementEntry item);
}

public sealed record AdaptiveEngagementEntryCeilingRangeRule(double Min, double Max) : IAdaptiveEngagementEntryRule
{
    public List<string> Check(AdaptiveEngagementEntry item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementEntryRequiredSourceRule(string Message) : IAdaptiveEngagementEntryRule
{
    public List<string> Check(AdaptiveEngagementEntry item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementEntryRateRangeRule(double Min, double Max) : IAdaptiveEngagementEntryRule
{
    public List<string> Check(AdaptiveEngagementEntry item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveEngagementEntryCodePrefixRule(string Prefix) : IAdaptiveEngagementEntryRule
{
    public List<string> Check(AdaptiveEngagementEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementEntryCodeLengthRule(int MaxLength) : IAdaptiveEngagementEntryRule
{
    public List<string> Check(AdaptiveEngagementEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive engagement entry.</summary>
public sealed record AdaptiveEngagementEntryValidator(List<IAdaptiveEngagementEntryRule> Rules)
{
    public List<string> Validate(AdaptiveEngagementEntry item)
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

    public void RequireValid(AdaptiveEngagementEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveEngagementEntry> items)
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

/// <summary>Functions of adaptive engagement entry rules.</summary>
public static class AdaptiveEngagementEntryRules
{
    public static AdaptiveEngagementEntryValidator DefaultAdaptiveEngagementEntryValidator()
    {
        return new AdaptiveEngagementEntryValidator(
            new List<IAdaptiveEngagementEntryRule>
            {
                new AdaptiveEngagementEntryCeilingRangeRule(2.8, 2348.0),
                new AdaptiveEngagementEntryRequiredSourceRule("adaptive engagement entry needs this field"),
                new AdaptiveEngagementEntryRateRangeRule(4.4, 1174.7),
                new AdaptiveEngagementEntryCodePrefixRule("OPS"),
                new AdaptiveEngagementEntryCodeLengthRule(120),
            });
    }
}
