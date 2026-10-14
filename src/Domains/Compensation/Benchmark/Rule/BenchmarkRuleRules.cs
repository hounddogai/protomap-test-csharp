namespace Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>A rule a benchmark rule must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkRuleRule
{
    List<string> Check(BenchmarkRule item);
}

public sealed record BenchmarkRuleLabelLengthRule(int MaxLength) : IBenchmarkRuleRule
{
    public List<string> Check(BenchmarkRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRuleRetriesRangeRule(int Min, int Max) : IBenchmarkRuleRule
{
    public List<string> Check(BenchmarkRule item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkRuleRequiredCodeRule(string Message) : IBenchmarkRuleRule
{
    public List<string> Check(BenchmarkRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRuleCodeLengthRule(int MaxLength) : IBenchmarkRuleRule
{
    public List<string> Check(BenchmarkRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRuleApprovedGuardRule(int Limit) : IBenchmarkRuleRule
{
    public List<string> Check(BenchmarkRule item)
    {
        if (item.Approved && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark rule.</summary>
public sealed record BenchmarkRuleValidator(List<IBenchmarkRuleRule> Rules)
{
    public List<string> Validate(BenchmarkRule item)
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

    public void RequireValid(BenchmarkRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkRule> items)
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

/// <summary>Functions of benchmark rule rules.</summary>
public static class BenchmarkRuleRules
{
    public static BenchmarkRuleValidator DefaultBenchmarkRuleValidator()
    {
        return new BenchmarkRuleValidator(
            new List<IBenchmarkRuleRule>
            {
                new BenchmarkRuleLabelLengthRule(16),
                new BenchmarkRuleRetriesRangeRule(64, 86400),
                new BenchmarkRuleRequiredCodeRule("benchmark rule needs an entry"),
                new BenchmarkRuleCodeLengthRule(32),
                new BenchmarkRuleApprovedGuardRule(80),
            });
    }
}
