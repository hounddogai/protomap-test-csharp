namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark budget must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkBudgetRule
{
    List<string> Check(BenchmarkBudget item);
}

public sealed record BenchmarkBudgetLimitRangeRule(int Min, int Max) : IBenchmarkBudgetRule
{
    public List<string> Check(BenchmarkBudget item)
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

public sealed record BenchmarkBudgetRequiredBadgeRule(string Message) : IBenchmarkBudgetRule
{
    public List<string> Check(BenchmarkBudget item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBudgetRequiredCodeRule(string Message) : IBenchmarkBudgetRule
{
    public List<string> Check(BenchmarkBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBudgetAllowedTitleRule(List<string> Allowed) : IBenchmarkBudgetRule
{
    public List<string> Check(BenchmarkBudget item)
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

/// <summary>Runs every rule of a benchmark budget.</summary>
public sealed record BenchmarkBudgetValidator(List<IBenchmarkBudgetRule> Rules)
{
    public List<string> Validate(BenchmarkBudget item)
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

    public void RequireValid(BenchmarkBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkBudget> items)
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

/// <summary>Functions of benchmark budget rules.</summary>
public static class BenchmarkBudgetRules
{
    public static BenchmarkBudgetValidator DefaultBenchmarkBudgetValidator()
    {
        return new BenchmarkBudgetValidator(
            new List<IBenchmarkBudgetRule>
            {
                new BenchmarkBudgetLimitRangeRule(32, 1000),
                new BenchmarkBudgetRequiredBadgeRule("benchmark budget needs this field"),
                new BenchmarkBudgetRequiredCodeRule("benchmark budget needs a value"),
                new BenchmarkBudgetAllowedTitleRule(new List<string> { "manual", "batched" }),
            });
    }
}
