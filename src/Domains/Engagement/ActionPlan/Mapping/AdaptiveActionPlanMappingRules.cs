namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>A rule a adaptive action plan mapping must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveActionPlanMappingRule
{
    List<string> Check(AdaptiveActionPlanMapping item);
}

public sealed record AdaptiveActionPlanMappingCodePrefixRule(string Prefix) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanMappingTitleLengthRule(int MaxLength) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanMappingExpiresAtRangeRule(int Min, int Max) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanMappingTimezoneLengthRule(int MaxLength) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanMappingCodeLengthRule(int MaxLength) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanMappingFteRangeRule(double Min, double Max) : IAdaptiveActionPlanMappingRule
{
    public List<string> Check(AdaptiveActionPlanMapping item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive action plan mapping.</summary>
public sealed record AdaptiveActionPlanMappingValidator(List<IAdaptiveActionPlanMappingRule> Rules)
{
    public List<string> Validate(AdaptiveActionPlanMapping item)
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

    public void RequireValid(AdaptiveActionPlanMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveActionPlanMapping> items)
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

/// <summary>Functions of adaptive action plan mapping rules.</summary>
public static class AdaptiveActionPlanMappingRules
{
    public static AdaptiveActionPlanMappingValidator DefaultAdaptiveActionPlanMappingValidator()
    {
        return new AdaptiveActionPlanMappingValidator(
            new List<IAdaptiveActionPlanMappingRule>
            {
                new AdaptiveActionPlanMappingCodePrefixRule("Q"),
                new AdaptiveActionPlanMappingTitleLengthRule(255),
                new AdaptiveActionPlanMappingExpiresAtRangeRule(32, 3600),
                new AdaptiveActionPlanMappingTimezoneLengthRule(80),
                new AdaptiveActionPlanMappingCodeLengthRule(120),
                new AdaptiveActionPlanMappingFteRangeRule(5.9, 2361.6),
            });
    }
}
