namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark bundle must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkBundleRule
{
    List<string> Check(BenchmarkBundle item);
}

public sealed record BenchmarkBundleWindowDaysRangeRule(int Min, int Max) : IBenchmarkBundleRule
{
    public List<string> Check(BenchmarkBundle item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkBundleCodePrefixRule(string Prefix) : IBenchmarkBundleRule
{
    public List<string> Check(BenchmarkBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBundleBatchSizeRangeRule(int Min, int Max) : IBenchmarkBundleRule
{
    public List<string> Check(BenchmarkBundle item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkBundleAllowedCodeRule(List<string> Allowed) : IBenchmarkBundleRule
{
    public List<string> Check(BenchmarkBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark bundle.</summary>
public sealed record BenchmarkBundleValidator(List<IBenchmarkBundleRule> Rules)
{
    public List<string> Validate(BenchmarkBundle item)
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

    public void RequireValid(BenchmarkBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkBundle> items)
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

/// <summary>Functions of benchmark bundle rules.</summary>
public static class BenchmarkBundleRules
{
    public static BenchmarkBundleValidator DefaultBenchmarkBundleValidator()
    {
        return new BenchmarkBundleValidator(
            new List<IBenchmarkBundleRule>
            {
                new BenchmarkBundleWindowDaysRangeRule(64, 80),
                new BenchmarkBundleCodePrefixRule("REF"),
                new BenchmarkBundleBatchSizeRangeRule(16, 120),
                new BenchmarkBundleAllowedCodeRule(new List<string> { "scheduled", "batched" }),
            });
    }
}
