namespace Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>A rule a legacy offboarding threshold must pass. Check returns its problems, or none.</summary>
public interface ILegacyOffboardingThresholdRule
{
    List<string> Check(LegacyOffboardingThreshold item);
}

public sealed record LegacyOffboardingThresholdVisibleGuardRule(double Limit) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        if (item.Visible && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingThresholdCodePrefixRule(string Prefix) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingThresholdRequiredTitleRule(string Message) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingThresholdRatioRangeRule(double Min, double Max) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingThresholdTitleLengthRule(int MaxLength) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyOffboardingThresholdRequiredCodeRule(string Message) : ILegacyOffboardingThresholdRule
{
    public List<string> Check(LegacyOffboardingThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy offboarding threshold.</summary>
public sealed record LegacyOffboardingThresholdValidator(List<ILegacyOffboardingThresholdRule> Rules)
{
    public List<string> Validate(LegacyOffboardingThreshold item)
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

    public void RequireValid(LegacyOffboardingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyOffboardingThreshold> items)
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

/// <summary>Functions of legacy offboarding threshold rules.</summary>
public static class LegacyOffboardingThresholdRules
{
    public static LegacyOffboardingThresholdValidator DefaultLegacyOffboardingThresholdValidator()
    {
        return new LegacyOffboardingThresholdValidator(
            new List<ILegacyOffboardingThresholdRule>
            {
                new LegacyOffboardingThresholdVisibleGuardRule(5.0),
                new LegacyOffboardingThresholdCodePrefixRule("OPS"),
                new LegacyOffboardingThresholdRequiredTitleRule("legacy offboarding threshold needs this field"),
                new LegacyOffboardingThresholdRatioRangeRule(0.7, 4283.0),
                new LegacyOffboardingThresholdTitleLengthRule(120),
                new LegacyOffboardingThresholdRequiredCodeRule("legacy offboarding threshold needs an entry"),
            });
    }
}
