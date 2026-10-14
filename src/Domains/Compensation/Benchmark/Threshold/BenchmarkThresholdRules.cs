namespace Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>A rule a benchmark threshold must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkThresholdRule
{
    List<string> Check(BenchmarkThreshold item);
}

public sealed record BenchmarkThresholdRequiredOwnerIdRule(string Message) : IBenchmarkThresholdRule
{
    public List<string> Check(BenchmarkThreshold item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkThresholdRequiredSkillRule(string Message) : IBenchmarkThresholdRule
{
    public List<string> Check(BenchmarkThreshold item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkThresholdCodeLengthRule(int MaxLength) : IBenchmarkThresholdRule
{
    public List<string> Check(BenchmarkThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark threshold.</summary>
public sealed record BenchmarkThresholdValidator(List<IBenchmarkThresholdRule> Rules)
{
    public List<string> Validate(BenchmarkThreshold item)
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

    public void RequireValid(BenchmarkThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkThreshold> items)
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

/// <summary>Functions of benchmark threshold rules.</summary>
public static class BenchmarkThresholdRules
{
    public static BenchmarkThresholdValidator DefaultBenchmarkThresholdValidator()
    {
        return new BenchmarkThresholdValidator(
            new List<IBenchmarkThresholdRule>
            {
                new BenchmarkThresholdRequiredOwnerIdRule("benchmark threshold needs this field"),
                new BenchmarkThresholdRequiredSkillRule("benchmark threshold needs this field"),
                new BenchmarkThresholdCodeLengthRule(32),
            });
    }
}
