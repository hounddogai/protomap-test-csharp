namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pay band review must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandReviewRule
{
    List<string> Check(ScheduledPayBandReview item);
}

public sealed record ScheduledPayBandReviewCodePrefixRule(string Prefix) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandReviewAllowedTitleRule(List<string> Allowed) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandReviewRequiredCodeRule(string Message) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandReviewCostCenterLengthRule(int MaxLength) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandReviewRequiredCostCenterRule(string Message) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandReviewChecksumLengthRule(int MaxLength) : IScheduledPayBandReviewRule
{
    public List<string> Check(ScheduledPayBandReview item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay band review.</summary>
public sealed record ScheduledPayBandReviewValidator(List<IScheduledPayBandReviewRule> Rules)
{
    public List<string> Validate(ScheduledPayBandReview item)
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

    public void RequireValid(ScheduledPayBandReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandReview> items)
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

/// <summary>Functions of scheduled pay band review rules.</summary>
public static class ScheduledPayBandReviewRules
{
    public static ScheduledPayBandReviewValidator DefaultScheduledPayBandReviewValidator()
    {
        return new ScheduledPayBandReviewValidator(
            new List<IScheduledPayBandReviewRule>
            {
                new ScheduledPayBandReviewCodePrefixRule("OPS"),
                new ScheduledPayBandReviewAllowedTitleRule(new List<string> { "scheduled" }),
                new ScheduledPayBandReviewRequiredCodeRule("scheduled pay band review needs an entry"),
                new ScheduledPayBandReviewCostCenterLengthRule(16),
                new ScheduledPayBandReviewRequiredCostCenterRule("scheduled pay band review needs an entry"),
                new ScheduledPayBandReviewChecksumLengthRule(255),
            });
    }
}
