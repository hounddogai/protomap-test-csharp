namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>A rule a tiered holiday review must pass. Check returns its problems, or none.</summary>
public interface ITieredHolidayReviewRule
{
    List<string> Check(TieredHolidayReview item);
}

public sealed record TieredHolidayReviewCodePrefixRule(string Prefix) : ITieredHolidayReviewRule
{
    public List<string> Check(TieredHolidayReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidayReviewAmountRangeRule(double Min, double Max) : ITieredHolidayReviewRule
{
    public List<string> Check(TieredHolidayReview item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidayReviewRequiredNameRule(string Message) : ITieredHolidayReviewRule
{
    public List<string> Check(TieredHolidayReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered holiday review.</summary>
public sealed record TieredHolidayReviewValidator(List<ITieredHolidayReviewRule> Rules)
{
    public List<string> Validate(TieredHolidayReview item)
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

    public void RequireValid(TieredHolidayReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredHolidayReview> items)
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

/// <summary>Functions of tiered holiday review rules.</summary>
public static class TieredHolidayReviewRules
{
    public static TieredHolidayReviewValidator DefaultTieredHolidayReviewValidator()
    {
        return new TieredHolidayReviewValidator(
            new List<ITieredHolidayReviewRule>
            {
                new TieredHolidayReviewCodePrefixRule("Q"),
                new TieredHolidayReviewAmountRangeRule(2.0, 2439.6),
                new TieredHolidayReviewRequiredNameRule("tiered holiday review needs an entry"),
            });
    }
}
