namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark batch must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkBatchRule
{
    List<string> Check(BenchmarkBatch item);
}

public sealed record BenchmarkBatchCreatedAtRangeRule(int Min, int Max) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkBatchAllowedLabelRule(List<string> Allowed) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBatchCodeLengthRule(int MaxLength) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBatchRequiredLabelRule(string Message) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBatchRequiredCodeRule(string Message) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBatchWindowDaysRangeRule(int Min, int Max) : IBenchmarkBatchRule
{
    public List<string> Check(BenchmarkBatch item)
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

/// <summary>Runs every rule of a benchmark batch.</summary>
public sealed record BenchmarkBatchValidator(List<IBenchmarkBatchRule> Rules)
{
    public List<string> Validate(BenchmarkBatch item)
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

    public void RequireValid(BenchmarkBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkBatch> items)
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

/// <summary>Functions of benchmark batch rules.</summary>
public static class BenchmarkBatchRules
{
    public static BenchmarkBatchValidator DefaultBenchmarkBatchValidator()
    {
        return new BenchmarkBatchValidator(
            new List<IBenchmarkBatchRule>
            {
                new BenchmarkBatchCreatedAtRangeRule(16, 255),
                new BenchmarkBatchAllowedLabelRule(new List<string>()),
                new BenchmarkBatchCodeLengthRule(32),
                new BenchmarkBatchRequiredLabelRule("benchmark batch needs an entry"),
                new BenchmarkBatchRequiredCodeRule("benchmark batch needs a value"),
                new BenchmarkBatchWindowDaysRangeRule(32, 120),
            });
    }
}
