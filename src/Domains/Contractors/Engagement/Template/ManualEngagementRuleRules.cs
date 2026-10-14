namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual engagement rule must pass. Check returns its problems, or none.</summary>
public interface IManualEngagementRuleRule
{
    List<string> Check(ManualEngagementRule item);
}

public sealed record ManualEngagementRuleVersionRangeRule(int Min, int Max) : IManualEngagementRuleRule
{
    public List<string> Check(ManualEngagementRule item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEngagementRuleStartsAtRangeRule(int Min, int Max) : IManualEngagementRuleRule
{
    public List<string> Check(ManualEngagementRule item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualEngagementRuleAllowedCodeRule(List<string> Allowed) : IManualEngagementRuleRule
{
    public List<string> Check(ManualEngagementRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual engagement rule.</summary>
public sealed record ManualEngagementRuleValidator(List<IManualEngagementRuleRule> Rules)
{
    public List<string> Validate(ManualEngagementRule item)
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

    public void RequireValid(ManualEngagementRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualEngagementRule> items)
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

/// <summary>Functions of manual engagement rule rules.</summary>
public static class ManualEngagementRuleRules
{
    public static ManualEngagementRuleValidator DefaultManualEngagementRuleValidator()
    {
        return new ManualEngagementRuleValidator(
            new List<IManualEngagementRuleRule>
            {
                new ManualEngagementRuleVersionRangeRule(32, 255),
                new ManualEngagementRuleStartsAtRangeRule(16, 255),
                new ManualEngagementRuleAllowedCodeRule(new List<string>()),
            });
    }
}
