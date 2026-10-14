namespace Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>A rule a lenient merit cycle review must pass. Check returns its problems, or none.</summary>
public interface ILenientMeritCycleReviewRule
{
    List<string> Check(LenientMeritCycleReview item);
}

public sealed record LenientMeritCycleReviewRequiredTitleRule(string Message) : ILenientMeritCycleReviewRule
{
    public List<string> Check(LenientMeritCycleReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleReviewRequiredUnitRule(string Message) : ILenientMeritCycleReviewRule
{
    public List<string> Check(LenientMeritCycleReview item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleReviewQuantityRangeRule(int Min, int Max) : ILenientMeritCycleReviewRule
{
    public List<string> Check(LenientMeritCycleReview item)
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

public sealed record LenientMeritCycleReviewCodePrefixRule(string Prefix) : ILenientMeritCycleReviewRule
{
    public List<string> Check(LenientMeritCycleReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleReviewRequiredCategoryRule(string Message) : ILenientMeritCycleReviewRule
{
    public List<string> Check(LenientMeritCycleReview item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient merit cycle review.</summary>
public sealed record LenientMeritCycleReviewValidator(List<ILenientMeritCycleReviewRule> Rules)
{
    public List<string> Validate(LenientMeritCycleReview item)
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

    public void RequireValid(LenientMeritCycleReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientMeritCycleReview> items)
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

/// <summary>Functions of lenient merit cycle review rules.</summary>
public static class LenientMeritCycleReviewRules
{
    public static LenientMeritCycleReviewValidator DefaultLenientMeritCycleReviewValidator()
    {
        return new LenientMeritCycleReviewValidator(
            new List<ILenientMeritCycleReviewRule>
            {
                new LenientMeritCycleReviewRequiredTitleRule("lenient merit cycle review needs a value"),
                new LenientMeritCycleReviewRequiredUnitRule("lenient merit cycle review needs a value"),
                new LenientMeritCycleReviewQuantityRangeRule(255, 255),
                new LenientMeritCycleReviewCodePrefixRule("X"),
                new LenientMeritCycleReviewRequiredCategoryRule("lenient merit cycle review needs a value"),
            });
    }
}
