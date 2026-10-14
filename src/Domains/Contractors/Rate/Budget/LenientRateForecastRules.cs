namespace Acme.Workforce.Contractors.Rate.Budget;

/// <summary>A rule a lenient rate forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientRateForecastRule
{
    List<string> Check(LenientRateForecast item);
}

public sealed record LenientRateForecastCodePrefixRule(string Prefix) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRateForecastApprovedGuardRule(double Limit) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        if (item.Approved && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LenientRateForecastRequiredCodeRule(string Message) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRateForecastRequiredReviewerRule(string Message) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRateForecastReviewerLengthRule(int MaxLength) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRateForecastRequiredLabelRule(string Message) : ILenientRateForecastRule
{
    public List<string> Check(LenientRateForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient rate forecast.</summary>
public sealed record LenientRateForecastValidator(List<ILenientRateForecastRule> Rules)
{
    public List<string> Validate(LenientRateForecast item)
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

    public void RequireValid(LenientRateForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRateForecast> items)
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

/// <summary>Functions of lenient rate forecast rules.</summary>
public static class LenientRateForecastRules
{
    public static LenientRateForecastValidator DefaultLenientRateForecastValidator()
    {
        return new LenientRateForecastValidator(
            new List<ILenientRateForecastRule>
            {
                new LenientRateForecastCodePrefixRule("Q"),
                new LenientRateForecastApprovedGuardRule(9.3),
                new LenientRateForecastRequiredCodeRule("lenient rate forecast needs a value"),
                new LenientRateForecastRequiredReviewerRule("lenient rate forecast needs this field"),
                new LenientRateForecastReviewerLengthRule(16),
                new LenientRateForecastRequiredLabelRule("lenient rate forecast needs a value"),
            });
    }
}
