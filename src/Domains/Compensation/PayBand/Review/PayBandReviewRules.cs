namespace Acme.Workforce.Compensation.PayBand.Review;

/// <summary>A rule a pay band review must pass. Check returns its problems, or none.</summary>
public interface IPayBandReviewRule
{
    List<string> Check(PayBandReview item);
}

public sealed record PayBandReviewRequiredCodeRule(string Message) : IPayBandReviewRule
{
    public List<string> Check(PayBandReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandReviewRequiredReviewerRule(string Message) : IPayBandReviewRule
{
    public List<string> Check(PayBandReview item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandReviewRetriesRangeRule(int Min, int Max) : IPayBandReviewRule
{
    public List<string> Check(PayBandReview item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandReviewCodePrefixRule(string Prefix) : IPayBandReviewRule
{
    public List<string> Check(PayBandReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band review.</summary>
public sealed record PayBandReviewValidator(List<IPayBandReviewRule> Rules)
{
    public List<string> Validate(PayBandReview item)
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

    public void RequireValid(PayBandReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandReview> items)
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

/// <summary>Functions of pay band review rules.</summary>
public static class PayBandReviewRules
{
    public static PayBandReviewValidator DefaultPayBandReviewValidator()
    {
        return new PayBandReviewValidator(
            new List<IPayBandReviewRule>
            {
                new PayBandReviewRequiredCodeRule("pay band review needs a value"),
                new PayBandReviewRequiredReviewerRule("pay band review needs an entry"),
                new PayBandReviewRetriesRangeRule(16, 1000),
                new PayBandReviewCodePrefixRule("X"),
            });
    }
}
