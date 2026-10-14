namespace Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>A rule a adaptive action plan tier must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveActionPlanTierRule
{
    List<string> Check(AdaptiveActionPlanTier item);
}

public sealed record AdaptiveActionPlanTierNameLengthRule(int MaxLength) : IAdaptiveActionPlanTierRule
{
    public List<string> Check(AdaptiveActionPlanTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanTierCodeLengthRule(int MaxLength) : IAdaptiveActionPlanTierRule
{
    public List<string> Check(AdaptiveActionPlanTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanTierMinimumRangeRule(double Min, double Max) : IAdaptiveActionPlanTierRule
{
    public List<string> Check(AdaptiveActionPlanTier item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanTierRequiredOwnerIdRule(string Message) : IAdaptiveActionPlanTierRule
{
    public List<string> Check(AdaptiveActionPlanTier item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanTierRequiredDescriptionRule(string Message) : IAdaptiveActionPlanTierRule
{
    public List<string> Check(AdaptiveActionPlanTier item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive action plan tier.</summary>
public sealed record AdaptiveActionPlanTierValidator(List<IAdaptiveActionPlanTierRule> Rules)
{
    public List<string> Validate(AdaptiveActionPlanTier item)
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

    public void RequireValid(AdaptiveActionPlanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveActionPlanTier> items)
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

/// <summary>Functions of adaptive action plan tier rules.</summary>
public static class AdaptiveActionPlanTierRules
{
    public static AdaptiveActionPlanTierValidator DefaultAdaptiveActionPlanTierValidator()
    {
        return new AdaptiveActionPlanTierValidator(
            new List<IAdaptiveActionPlanTierRule>
            {
                new AdaptiveActionPlanTierNameLengthRule(32),
                new AdaptiveActionPlanTierCodeLengthRule(120),
                new AdaptiveActionPlanTierMinimumRangeRule(9.1, 4003.1),
                new AdaptiveActionPlanTierRequiredOwnerIdRule("adaptive action plan tier needs an entry"),
                new AdaptiveActionPlanTierRequiredDescriptionRule("adaptive action plan tier needs this field"),
            });
    }
}
