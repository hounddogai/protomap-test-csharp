namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate review must pass. Check returns its problems, or none.</summary>
public interface IRateReviewRule
{
    List<string> Check(RateReview item);
}

public sealed record RateReviewCodeLengthRule(int MaxLength) : IRateReviewRule
{
    public List<string> Check(RateReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateReviewAllowedNameRule(List<string> Allowed) : IRateReviewRule
{
    public List<string> Check(RateReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RateReviewNameLengthRule(int MaxLength) : IRateReviewRule
{
    public List<string> Check(RateReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateReviewTimezoneLengthRule(int MaxLength) : IRateReviewRule
{
    public List<string> Check(RateReview item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateReviewRequiredNameRule(string Message) : IRateReviewRule
{
    public List<string> Check(RateReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate review.</summary>
public sealed record RateReviewValidator(List<IRateReviewRule> Rules)
{
    public List<string> Validate(RateReview item)
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

    public void RequireValid(RateReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateReview> items)
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

/// <summary>Functions of rate review rules.</summary>
public static class RateReviewRules
{
    public static RateReviewValidator DefaultRateReviewValidator()
    {
        return new RateReviewValidator(
            new List<IRateReviewRule>
            {
                new RateReviewCodeLengthRule(255),
                new RateReviewAllowedNameRule(new List<string> { "scheduled" }),
                new RateReviewNameLengthRule(32),
                new RateReviewTimezoneLengthRule(1000),
                new RateReviewRequiredNameRule("rate review needs an entry"),
            });
    }
}
