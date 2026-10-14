namespace Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>A rule a benchmark snapshot must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkSnapshotRule
{
    List<string> Check(BenchmarkSnapshot item);
}

public sealed record BenchmarkSnapshotRequiredCodeRule(string Message) : IBenchmarkSnapshotRule
{
    public List<string> Check(BenchmarkSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSnapshotLimitRangeRule(int Min, int Max) : IBenchmarkSnapshotRule
{
    public List<string> Check(BenchmarkSnapshot item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkSnapshotCountryLengthRule(int MaxLength) : IBenchmarkSnapshotRule
{
    public List<string> Check(BenchmarkSnapshot item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSnapshotCategoryLengthRule(int MaxLength) : IBenchmarkSnapshotRule
{
    public List<string> Check(BenchmarkSnapshot item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark snapshot.</summary>
public sealed record BenchmarkSnapshotValidator(List<IBenchmarkSnapshotRule> Rules)
{
    public List<string> Validate(BenchmarkSnapshot item)
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

    public void RequireValid(BenchmarkSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkSnapshot> items)
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

/// <summary>Functions of benchmark snapshot rules.</summary>
public static class BenchmarkSnapshotRules
{
    public static BenchmarkSnapshotValidator DefaultBenchmarkSnapshotValidator()
    {
        return new BenchmarkSnapshotValidator(
            new List<IBenchmarkSnapshotRule>
            {
                new BenchmarkSnapshotRequiredCodeRule("benchmark snapshot needs this field"),
                new BenchmarkSnapshotLimitRangeRule(32, 3600),
                new BenchmarkSnapshotCountryLengthRule(16),
                new BenchmarkSnapshotCategoryLengthRule(64),
            });
    }
}
