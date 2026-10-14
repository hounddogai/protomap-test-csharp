namespace Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>A rule a regional benchmark review must pass. Check returns its problems, or none.</summary>
public interface IRegionalBenchmarkReviewRule
{
    List<string> Check(RegionalBenchmarkReview item);
}

public sealed record RegionalBenchmarkReviewCodePrefixRule(string Prefix) : IRegionalBenchmarkReviewRule
{
    public List<string> Check(RegionalBenchmarkReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkReviewPhoneLengthRule(int MaxLength) : IRegionalBenchmarkReviewRule
{
    public List<string> Check(RegionalBenchmarkReview item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkReviewCodeLengthRule(int MaxLength) : IRegionalBenchmarkReviewRule
{
    public List<string> Check(RegionalBenchmarkReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkReviewMinimumRangeRule(double Min, double Max) : IRegionalBenchmarkReviewRule
{
    public List<string> Check(RegionalBenchmarkReview item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalBenchmarkReviewRateRangeRule(double Min, double Max) : IRegionalBenchmarkReviewRule
{
    public List<string> Check(RegionalBenchmarkReview item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional benchmark review.</summary>
public sealed record RegionalBenchmarkReviewValidator(List<IRegionalBenchmarkReviewRule> Rules)
{
    public List<string> Validate(RegionalBenchmarkReview item)
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

    public void RequireValid(RegionalBenchmarkReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBenchmarkReview> items)
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

/// <summary>Functions of regional benchmark review rules.</summary>
public static class RegionalBenchmarkReviewRules
{
    public static RegionalBenchmarkReviewValidator DefaultRegionalBenchmarkReviewValidator()
    {
        return new RegionalBenchmarkReviewValidator(
            new List<IRegionalBenchmarkReviewRule>
            {
                new RegionalBenchmarkReviewCodePrefixRule("ACM"),
                new RegionalBenchmarkReviewPhoneLengthRule(16),
                new RegionalBenchmarkReviewCodeLengthRule(255),
                new RegionalBenchmarkReviewMinimumRangeRule(1.7, 4890.3),
                new RegionalBenchmarkReviewRateRangeRule(1.6, 2788.9),
            });
    }
}
