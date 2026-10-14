namespace Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>A rule a benchmark summary must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkSummaryRule
{
    List<string> Check(BenchmarkSummary item);
}

public sealed record BenchmarkSummaryBudgetRangeRule(double Min, double Max) : IBenchmarkSummaryRule
{
    public List<string> Check(BenchmarkSummary item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkSummaryRequiredLabelRule(string Message) : IBenchmarkSummaryRule
{
    public List<string> Check(BenchmarkSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSummaryCodeLengthRule(int MaxLength) : IBenchmarkSummaryRule
{
    public List<string> Check(BenchmarkSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSummaryRequiredCodeRule(string Message) : IBenchmarkSummaryRule
{
    public List<string> Check(BenchmarkSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark summary.</summary>
public sealed record BenchmarkSummaryValidator(List<IBenchmarkSummaryRule> Rules)
{
    public List<string> Validate(BenchmarkSummary item)
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

    public void RequireValid(BenchmarkSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkSummary> items)
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

/// <summary>Functions of benchmark summary rules.</summary>
public static class BenchmarkSummaryRules
{
    public static BenchmarkSummaryValidator DefaultBenchmarkSummaryValidator()
    {
        return new BenchmarkSummaryValidator(
            new List<IBenchmarkSummaryRule>
            {
                new BenchmarkSummaryBudgetRangeRule(5.6, 2638.0),
                new BenchmarkSummaryRequiredLabelRule("benchmark summary needs an entry"),
                new BenchmarkSummaryCodeLengthRule(16),
                new BenchmarkSummaryRequiredCodeRule("benchmark summary needs an entry"),
            });
    }
}
