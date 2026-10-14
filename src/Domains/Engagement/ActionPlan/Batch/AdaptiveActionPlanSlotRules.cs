namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive action plan slot must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveActionPlanSlotRule
{
    List<string> Check(AdaptiveActionPlanSlot item);
}

public sealed record AdaptiveActionPlanSlotTitleLengthRule(int MaxLength) : IAdaptiveActionPlanSlotRule
{
    public List<string> Check(AdaptiveActionPlanSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanSlotScoreRangeRule(double Min, double Max) : IAdaptiveActionPlanSlotRule
{
    public List<string> Check(AdaptiveActionPlanSlot item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanSlotAllowedBadgeRule(List<string> Allowed) : IAdaptiveActionPlanSlotRule
{
    public List<string> Check(AdaptiveActionPlanSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanSlotRequiredBadgeRule(string Message) : IAdaptiveActionPlanSlotRule
{
    public List<string> Check(AdaptiveActionPlanSlot item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanSlotCodePrefixRule(string Prefix) : IAdaptiveActionPlanSlotRule
{
    public List<string> Check(AdaptiveActionPlanSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive action plan slot.</summary>
public sealed record AdaptiveActionPlanSlotValidator(List<IAdaptiveActionPlanSlotRule> Rules)
{
    public List<string> Validate(AdaptiveActionPlanSlot item)
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

    public void RequireValid(AdaptiveActionPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveActionPlanSlot> items)
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

/// <summary>Functions of adaptive action plan slot rules.</summary>
public static class AdaptiveActionPlanSlotRules
{
    public static AdaptiveActionPlanSlotValidator DefaultAdaptiveActionPlanSlotValidator()
    {
        return new AdaptiveActionPlanSlotValidator(
            new List<IAdaptiveActionPlanSlotRule>
            {
                new AdaptiveActionPlanSlotTitleLengthRule(16),
                new AdaptiveActionPlanSlotScoreRangeRule(0.0, 3716.3),
                new AdaptiveActionPlanSlotAllowedBadgeRule(new List<string> { "strict" }),
                new AdaptiveActionPlanSlotRequiredBadgeRule("adaptive action plan slot needs this field"),
                new AdaptiveActionPlanSlotCodePrefixRule("REF"),
            });
    }
}
