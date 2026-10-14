namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal deduction review must pass. Check returns its problems, or none.</summary>
public interface ISeasonalDeductionReviewRule
{
    List<string> Check(SeasonalDeductionReview item);
}

public sealed record SeasonalDeductionReviewRequiredEmployeeIdRule(string Message) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionReviewCeilingRangeRule(double Min, double Max) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionReviewRequiredRegionRule(string Message) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionReviewCodePrefixRule(string Prefix) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionReviewAllowedRegionRule(List<string> Allowed) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionReviewRequiredLabelRule(string Message) : ISeasonalDeductionReviewRule
{
    public List<string> Check(SeasonalDeductionReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal deduction review.</summary>
public sealed record SeasonalDeductionReviewValidator(List<ISeasonalDeductionReviewRule> Rules)
{
    public List<string> Validate(SeasonalDeductionReview item)
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

    public void RequireValid(SeasonalDeductionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalDeductionReview> items)
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

/// <summary>Functions of seasonal deduction review rules.</summary>
public static class SeasonalDeductionReviewRules
{
    public static SeasonalDeductionReviewValidator DefaultSeasonalDeductionReviewValidator()
    {
        return new SeasonalDeductionReviewValidator(
            new List<ISeasonalDeductionReviewRule>
            {
                new SeasonalDeductionReviewRequiredEmployeeIdRule("seasonal deduction review needs an entry"),
                new SeasonalDeductionReviewCeilingRangeRule(5.5, 2293.7),
                new SeasonalDeductionReviewRequiredRegionRule("seasonal deduction review needs this field"),
                new SeasonalDeductionReviewCodePrefixRule("REF"),
                new SeasonalDeductionReviewAllowedRegionRule(new List<string> { "seasonal" }),
                new SeasonalDeductionReviewRequiredLabelRule("seasonal deduction review needs an entry"),
            });
    }
}
