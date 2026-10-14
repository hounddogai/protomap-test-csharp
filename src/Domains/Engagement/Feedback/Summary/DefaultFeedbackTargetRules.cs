namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default feedback target must pass. Check returns its problems, or none.</summary>
public interface IDefaultFeedbackTargetRule
{
    List<string> Check(DefaultFeedbackTarget item);
}

public sealed record DefaultFeedbackTargetMarginRangeRule(double Min, double Max) : IDefaultFeedbackTargetRule
{
    public List<string> Check(DefaultFeedbackTarget item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultFeedbackTargetRequiredExternalRefRule(string Message) : IDefaultFeedbackTargetRule
{
    public List<string> Check(DefaultFeedbackTarget item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultFeedbackTargetAllowedTitleRule(List<string> Allowed) : IDefaultFeedbackTargetRule
{
    public List<string> Check(DefaultFeedbackTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFeedbackTargetRequiredTitleRule(string Message) : IDefaultFeedbackTargetRule
{
    public List<string> Check(DefaultFeedbackTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default feedback target.</summary>
public sealed record DefaultFeedbackTargetValidatorExtra(List<IDefaultFeedbackTargetRule> Rules)
{
    public List<string> Validate(DefaultFeedbackTarget item)
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

    public void RequireValid(DefaultFeedbackTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultFeedbackTarget> items)
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

/// <summary>Functions of default feedback target rules.</summary>
public static class DefaultFeedbackTargetRules
{
    public static DefaultFeedbackTargetValidatorExtra DefaultDefaultFeedbackTargetValidator()
    {
        return new DefaultFeedbackTargetValidatorExtra(
            new List<IDefaultFeedbackTargetRule>
            {
                new DefaultFeedbackTargetMarginRangeRule(0.2, 352.0),
                new DefaultFeedbackTargetRequiredExternalRefRule("default feedback target needs an entry"),
                new DefaultFeedbackTargetAllowedTitleRule(new List<string> { "tiered", "global" }),
                new DefaultFeedbackTargetRequiredTitleRule("default feedback target needs this field"),
            });
    }
}
