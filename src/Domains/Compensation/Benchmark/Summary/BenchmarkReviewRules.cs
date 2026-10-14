namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark review must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkReviewRule
{
    List<string> Check(BenchmarkReview item);
}

public sealed record BenchmarkReviewAllowedDepartmentRule(List<string> Allowed) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReviewDiscountRangeRule(double Min, double Max) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReviewCodeLengthRule(int MaxLength) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReviewCodePrefixRule(string Prefix) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReviewMarginRangeRule(double Min, double Max) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkReviewRequiredLocationRule(string Message) : IBenchmarkReviewRule
{
    public List<string> Check(BenchmarkReview item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark review.</summary>
public sealed record BenchmarkReviewValidator(List<IBenchmarkReviewRule> Rules)
{
    public List<string> Validate(BenchmarkReview item)
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

    public void RequireValid(BenchmarkReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkReview> items)
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

/// <summary>Functions of benchmark review rules.</summary>
public static class BenchmarkReviewRules
{
    public static BenchmarkReviewValidator DefaultBenchmarkReviewValidator()
    {
        return new BenchmarkReviewValidator(
            new List<IBenchmarkReviewRule>
            {
                new BenchmarkReviewAllowedDepartmentRule(new List<string> { "scheduled", "rolling" }),
                new BenchmarkReviewDiscountRangeRule(8.1, 2639.5),
                new BenchmarkReviewCodeLengthRule(86400),
                new BenchmarkReviewCodePrefixRule("REF"),
                new BenchmarkReviewMarginRangeRule(5.4, 1585.8),
                new BenchmarkReviewRequiredLocationRule("benchmark review needs a value"),
            });
    }
}
