namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive feedback summary must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFeedbackSummaryRule
{
    List<string> Check(AdaptiveFeedbackSummary item);
}

public sealed record AdaptiveFeedbackSummaryRequiredCodeRule(string Message) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackSummaryAllowedCodeRule(List<string> Allowed) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackSummaryTitleLengthRule(int MaxLength) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackSummaryCodeLengthRule(int MaxLength) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackSummaryCodePrefixRule(string Prefix) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackSummaryHoursRangeRule(double Min, double Max) : IAdaptiveFeedbackSummaryRule
{
    public List<string> Check(AdaptiveFeedbackSummary item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive feedback summary.</summary>
public sealed record AdaptiveFeedbackSummaryValidator(List<IAdaptiveFeedbackSummaryRule> Rules)
{
    public List<string> Validate(AdaptiveFeedbackSummary item)
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

    public void RequireValid(AdaptiveFeedbackSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFeedbackSummary> items)
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

/// <summary>Functions of adaptive feedback summary rules.</summary>
public static class AdaptiveFeedbackSummaryRules
{
    public static AdaptiveFeedbackSummaryValidator DefaultAdaptiveFeedbackSummaryValidator()
    {
        return new AdaptiveFeedbackSummaryValidator(
            new List<IAdaptiveFeedbackSummaryRule>
            {
                new AdaptiveFeedbackSummaryRequiredCodeRule("adaptive feedback summary needs this field"),
                new AdaptiveFeedbackSummaryAllowedCodeRule(new List<string>()),
                new AdaptiveFeedbackSummaryTitleLengthRule(1000),
                new AdaptiveFeedbackSummaryCodeLengthRule(16),
                new AdaptiveFeedbackSummaryCodePrefixRule("Q"),
                new AdaptiveFeedbackSummaryHoursRangeRule(9.7, 158.3),
            });
    }
}
