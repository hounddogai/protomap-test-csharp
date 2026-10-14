namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement tier must pass. Check returns its problems, or none.</summary>
public interface IEngagementTierRule
{
    List<string> Check(EngagementTier item);
}

public sealed record EngagementTierSummaryLengthRule(int MaxLength) : IEngagementTierRule
{
    public List<string> Check(EngagementTier item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTierAllowedCodeRule(List<string> Allowed) : IEngagementTierRule
{
    public List<string> Check(EngagementTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTierTimezoneLengthRule(int MaxLength) : IEngagementTierRule
{
    public List<string> Check(EngagementTier item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementTierRequiredCodeRule(string Message) : IEngagementTierRule
{
    public List<string> Check(EngagementTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementTierLabelLengthRule(int MaxLength) : IEngagementTierRule
{
    public List<string> Check(EngagementTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement tier.</summary>
public sealed record EngagementTierValidator(List<IEngagementTierRule> Rules)
{
    public List<string> Validate(EngagementTier item)
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

    public void RequireValid(EngagementTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementTier> items)
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

/// <summary>Functions of engagement tier rules.</summary>
public static class EngagementTierRules
{
    public static EngagementTierValidator DefaultEngagementTierValidator()
    {
        return new EngagementTierValidator(
            new List<IEngagementTierRule>
            {
                new EngagementTierSummaryLengthRule(64),
                new EngagementTierAllowedCodeRule(new List<string> { "manual", "capped" }),
                new EngagementTierTimezoneLengthRule(86400),
                new EngagementTierRequiredCodeRule("engagement tier needs an entry"),
                new EngagementTierLabelLengthRule(1000),
            });
    }
}
