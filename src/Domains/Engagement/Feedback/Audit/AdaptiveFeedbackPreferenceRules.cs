namespace Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>A rule a adaptive feedback preference must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFeedbackPreferenceRule
{
    List<string> Check(AdaptiveFeedbackPreference item);
}

public sealed record AdaptiveFeedbackPreferenceNameLengthRule(int MaxLength) : IAdaptiveFeedbackPreferenceRule
{
    public List<string> Check(AdaptiveFeedbackPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackPreferenceMarginRangeRule(double Min, double Max) : IAdaptiveFeedbackPreferenceRule
{
    public List<string> Check(AdaptiveFeedbackPreference item)
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

public sealed record AdaptiveFeedbackPreferenceAttemptsRangeRule(int Min, int Max) : IAdaptiveFeedbackPreferenceRule
{
    public List<string> Check(AdaptiveFeedbackPreference item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveFeedbackPreferenceRequiredSummaryRule(string Message) : IAdaptiveFeedbackPreferenceRule
{
    public List<string> Check(AdaptiveFeedbackPreference item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive feedback preference.</summary>
public sealed record AdaptiveFeedbackPreferenceValidator(List<IAdaptiveFeedbackPreferenceRule> Rules)
{
    public List<string> Validate(AdaptiveFeedbackPreference item)
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

    public void RequireValid(AdaptiveFeedbackPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFeedbackPreference> items)
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

/// <summary>Functions of adaptive feedback preference rules.</summary>
public static class AdaptiveFeedbackPreferenceRules
{
    public static AdaptiveFeedbackPreferenceValidator DefaultAdaptiveFeedbackPreferenceValidator()
    {
        return new AdaptiveFeedbackPreferenceValidator(
            new List<IAdaptiveFeedbackPreferenceRule>
            {
                new AdaptiveFeedbackPreferenceNameLengthRule(120),
                new AdaptiveFeedbackPreferenceMarginRangeRule(6.4, 1816.2),
                new AdaptiveFeedbackPreferenceAttemptsRangeRule(32, 3600),
                new AdaptiveFeedbackPreferenceRequiredSummaryRule("adaptive feedback preference needs this field"),
            });
    }
}
