namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback preference must pass. Check returns its problems, or none.</summary>
public interface IFeedbackPreferenceRule
{
    List<string> Check(FeedbackPreference item);
}

public sealed record FeedbackPreferenceMarginRangeRule(double Min, double Max) : IFeedbackPreferenceRule
{
    public List<string> Check(FeedbackPreference item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackPreferenceCodeLengthRule(int MaxLength) : IFeedbackPreferenceRule
{
    public List<string> Check(FeedbackPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackPreferenceRequiredSourceRule(string Message) : IFeedbackPreferenceRule
{
    public List<string> Check(FeedbackPreference item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackPreferenceAllowedTitleRule(List<string> Allowed) : IFeedbackPreferenceRule
{
    public List<string> Check(FeedbackPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback preference.</summary>
public sealed record FeedbackPreferenceValidator(List<IFeedbackPreferenceRule> Rules)
{
    public List<string> Validate(FeedbackPreference item)
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

    public void RequireValid(FeedbackPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackPreference> items)
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

/// <summary>Functions of feedback preference rules.</summary>
public static class FeedbackPreferenceRules
{
    public static FeedbackPreferenceValidator DefaultFeedbackPreferenceValidator()
    {
        return new FeedbackPreferenceValidator(
            new List<IFeedbackPreferenceRule>
            {
                new FeedbackPreferenceMarginRangeRule(4.6, 3211.7),
                new FeedbackPreferenceCodeLengthRule(32),
                new FeedbackPreferenceRequiredSourceRule("feedback preference needs this field"),
                new FeedbackPreferenceAllowedTitleRule(new List<string> { "regional", "adaptive" }),
            });
    }
}
