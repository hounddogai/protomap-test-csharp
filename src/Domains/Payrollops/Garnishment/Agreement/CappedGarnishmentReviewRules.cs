namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>A rule a capped garnishment review must pass. Check returns its problems, or none.</summary>
public interface ICappedGarnishmentReviewRule
{
    List<string> Check(CappedGarnishmentReview item);
}

public sealed record CappedGarnishmentReviewQuantityRangeRule(int Min, int Max) : ICappedGarnishmentReviewRule
{
    public List<string> Check(CappedGarnishmentReview item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedGarnishmentReviewFloorValueRangeRule(double Min, double Max) : ICappedGarnishmentReviewRule
{
    public List<string> Check(CappedGarnishmentReview item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedGarnishmentReviewRequiredCodeRule(string Message) : ICappedGarnishmentReviewRule
{
    public List<string> Check(CappedGarnishmentReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedGarnishmentReviewCodePrefixRule(string Prefix) : ICappedGarnishmentReviewRule
{
    public List<string> Check(CappedGarnishmentReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped garnishment review.</summary>
public sealed record CappedGarnishmentReviewValidator(List<ICappedGarnishmentReviewRule> Rules)
{
    public List<string> Validate(CappedGarnishmentReview item)
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

    public void RequireValid(CappedGarnishmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedGarnishmentReview> items)
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

/// <summary>Functions of capped garnishment review rules.</summary>
public static class CappedGarnishmentReviewRules
{
    public static CappedGarnishmentReviewValidator DefaultCappedGarnishmentReviewValidator()
    {
        return new CappedGarnishmentReviewValidator(
            new List<ICappedGarnishmentReviewRule>
            {
                new CappedGarnishmentReviewQuantityRangeRule(16, 86400),
                new CappedGarnishmentReviewFloorValueRangeRule(6.5, 2113.3),
                new CappedGarnishmentReviewRequiredCodeRule("capped garnishment review needs an entry"),
                new CappedGarnishmentReviewCodePrefixRule("Q"),
            });
    }
}
