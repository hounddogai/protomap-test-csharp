namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling span review must pass. Check returns its problems, or none.</summary>
public interface IRollingSpanReviewRule
{
    List<string> Check(RollingSpanReview item);
}

public sealed record RollingSpanReviewCodePrefixRule(string Prefix) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanReviewRequiredCodeRule(string Message) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanReviewRateRangeRule(double Min, double Max) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSpanReviewRequiredTitleRule(string Message) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanReviewRequiredCostCenterRule(string Message) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanReviewAllowedTitleRule(List<string> Allowed) : IRollingSpanReviewRule
{
    public List<string> Check(RollingSpanReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling span review.</summary>
public sealed record RollingSpanReviewValidator(List<IRollingSpanReviewRule> Rules)
{
    public List<string> Validate(RollingSpanReview item)
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

    public void RequireValid(RollingSpanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSpanReview> items)
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

/// <summary>Functions of rolling span review rules.</summary>
public static class RollingSpanReviewRules
{
    public static RollingSpanReviewValidator DefaultRollingSpanReviewValidator()
    {
        return new RollingSpanReviewValidator(
            new List<IRollingSpanReviewRule>
            {
                new RollingSpanReviewCodePrefixRule("REF"),
                new RollingSpanReviewRequiredCodeRule("rolling span review needs an entry"),
                new RollingSpanReviewRateRangeRule(5.0, 1807.6),
                new RollingSpanReviewRequiredTitleRule("rolling span review needs an entry"),
                new RollingSpanReviewRequiredCostCenterRule("rolling span review needs an entry"),
                new RollingSpanReviewAllowedTitleRule(new List<string> { "default" }),
            });
    }
}
