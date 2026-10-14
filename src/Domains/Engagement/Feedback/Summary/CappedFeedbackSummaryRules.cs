namespace Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>A rule a capped feedback summary must pass. Check returns its problems, or none.</summary>
public interface ICappedFeedbackSummaryRule
{
    List<string> Check(CappedFeedbackSummary item);
}

public sealed record CappedFeedbackSummaryRequiredContactEmailRule(string Message) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackSummaryCodePrefixRule(string Prefix) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackSummaryCodeLengthRule(int MaxLength) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackSummaryCostRangeRule(double Min, double Max) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedFeedbackSummaryRequiredDescriptionRule(string Message) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedFeedbackSummaryDescriptionLengthRule(int MaxLength) : ICappedFeedbackSummaryRule
{
    public List<string> Check(CappedFeedbackSummary item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped feedback summary.</summary>
public sealed record CappedFeedbackSummaryValidator(List<ICappedFeedbackSummaryRule> Rules)
{
    public List<string> Validate(CappedFeedbackSummary item)
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

    public void RequireValid(CappedFeedbackSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedFeedbackSummary> items)
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

/// <summary>Functions of capped feedback summary rules.</summary>
public static class CappedFeedbackSummaryRules
{
    public static CappedFeedbackSummaryValidator DefaultCappedFeedbackSummaryValidator()
    {
        return new CappedFeedbackSummaryValidator(
            new List<ICappedFeedbackSummaryRule>
            {
                new CappedFeedbackSummaryRequiredContactEmailRule("capped feedback summary needs an entry"),
                new CappedFeedbackSummaryCodePrefixRule("X"),
                new CappedFeedbackSummaryCodeLengthRule(16),
                new CappedFeedbackSummaryCostRangeRule(1.2, 4477.5),
                new CappedFeedbackSummaryRequiredDescriptionRule("capped feedback summary needs a value"),
                new CappedFeedbackSummaryDescriptionLengthRule(255),
            });
    }
}
