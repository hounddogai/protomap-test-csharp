namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive action plan entry must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveActionPlanEntryRule
{
    List<string> Check(AdaptiveActionPlanEntry item);
}

public sealed record AdaptiveActionPlanEntryLabelLengthRule(int MaxLength) : IAdaptiveActionPlanEntryRule
{
    public List<string> Check(AdaptiveActionPlanEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanEntryRequiredLabelRule(string Message) : IAdaptiveActionPlanEntryRule
{
    public List<string> Check(AdaptiveActionPlanEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveActionPlanEntryAllowedCodeRule(List<string> Allowed) : IAdaptiveActionPlanEntryRule
{
    public List<string> Check(AdaptiveActionPlanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive action plan entry.</summary>
public sealed record AdaptiveActionPlanEntryValidator(List<IAdaptiveActionPlanEntryRule> Rules)
{
    public List<string> Validate(AdaptiveActionPlanEntry item)
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

    public void RequireValid(AdaptiveActionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveActionPlanEntry> items)
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

/// <summary>Functions of adaptive action plan entry rules.</summary>
public static class AdaptiveActionPlanEntryRules
{
    public static AdaptiveActionPlanEntryValidator DefaultAdaptiveActionPlanEntryValidator()
    {
        return new AdaptiveActionPlanEntryValidator(
            new List<IAdaptiveActionPlanEntryRule>
            {
                new AdaptiveActionPlanEntryLabelLengthRule(120),
                new AdaptiveActionPlanEntryRequiredLabelRule("adaptive action plan entry needs this field"),
                new AdaptiveActionPlanEntryAllowedCodeRule(new List<string>()),
            });
    }
}
