namespace Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>A rule a adaptive engagement notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveEngagementNoticeRule
{
    List<string> Check(AdaptiveEngagementNotice item);
}

public sealed record AdaptiveEngagementNoticeActiveGuardRule(double Limit) : IAdaptiveEngagementNoticeRule
{
    public List<string> Check(AdaptiveEngagementNotice item)
    {
        if (item.Active && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementNoticeCodePrefixRule(string Prefix) : IAdaptiveEngagementNoticeRule
{
    public List<string> Check(AdaptiveEngagementNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementNoticeRequiredTitleRule(string Message) : IAdaptiveEngagementNoticeRule
{
    public List<string> Check(AdaptiveEngagementNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementNoticeMinimumRangeRule(double Min, double Max) : IAdaptiveEngagementNoticeRule
{
    public List<string> Check(AdaptiveEngagementNotice item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive engagement notice.</summary>
public sealed record AdaptiveEngagementNoticeValidator(List<IAdaptiveEngagementNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveEngagementNotice item)
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

    public void RequireValid(AdaptiveEngagementNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveEngagementNotice> items)
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

/// <summary>Functions of adaptive engagement notice rules.</summary>
public static class AdaptiveEngagementNoticeRules
{
    public static AdaptiveEngagementNoticeValidator DefaultAdaptiveEngagementNoticeValidator()
    {
        return new AdaptiveEngagementNoticeValidator(
            new List<IAdaptiveEngagementNoticeRule>
            {
                new AdaptiveEngagementNoticeActiveGuardRule(2.8),
                new AdaptiveEngagementNoticeCodePrefixRule("REF"),
                new AdaptiveEngagementNoticeRequiredTitleRule("adaptive engagement notice needs this field"),
                new AdaptiveEngagementNoticeMinimumRangeRule(7.9, 4602.4),
            });
    }
}
