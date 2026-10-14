namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>A rule a regional span review must pass. Check returns its problems, or none.</summary>
public interface IRegionalSpanReviewRule
{
    List<string> Check(RegionalSpanReview item);
}

public sealed record RegionalSpanReviewFloorValueRangeRule(double Min, double Max) : IRegionalSpanReviewRule
{
    public List<string> Check(RegionalSpanReview item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSpanReviewRequiredCurrencyRule(string Message) : IRegionalSpanReviewRule
{
    public List<string> Check(RegionalSpanReview item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanReviewExpiresAtRangeRule(int Min, int Max) : IRegionalSpanReviewRule
{
    public List<string> Check(RegionalSpanReview item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional span review.</summary>
public sealed record RegionalSpanReviewValidator(List<IRegionalSpanReviewRule> Rules)
{
    public List<string> Validate(RegionalSpanReview item)
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

    public void RequireValid(RegionalSpanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSpanReview> items)
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

/// <summary>Functions of regional span review rules.</summary>
public static class RegionalSpanReviewRules
{
    public static RegionalSpanReviewValidator DefaultRegionalSpanReviewValidator()
    {
        return new RegionalSpanReviewValidator(
            new List<IRegionalSpanReviewRule>
            {
                new RegionalSpanReviewFloorValueRangeRule(7.0, 1976.2),
                new RegionalSpanReviewRequiredCurrencyRule("regional span review needs an entry"),
                new RegionalSpanReviewExpiresAtRangeRule(16, 80),
            });
    }
}
