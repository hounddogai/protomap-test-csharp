namespace Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>A rule a benchmark plan must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkPlanRule
{
    List<string> Check(BenchmarkPlan item);
}

public sealed record BenchmarkPlanThresholdRangeRule(double Min, double Max) : IBenchmarkPlanRule
{
    public List<string> Check(BenchmarkPlan item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPlanCodeLengthRule(int MaxLength) : IBenchmarkPlanRule
{
    public List<string> Check(BenchmarkPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPlanRequiredLabelRule(string Message) : IBenchmarkPlanRule
{
    public List<string> Check(BenchmarkPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPlanRequiredJobCodeRule(string Message) : IBenchmarkPlanRule
{
    public List<string> Check(BenchmarkPlan item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPlanRequiredCodeRule(string Message) : IBenchmarkPlanRule
{
    public List<string> Check(BenchmarkPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark plan.</summary>
public sealed record BenchmarkPlanValidator(List<IBenchmarkPlanRule> Rules)
{
    public List<string> Validate(BenchmarkPlan item)
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

    public void RequireValid(BenchmarkPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkPlan> items)
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

/// <summary>Functions of benchmark plan rules.</summary>
public static class BenchmarkPlanRules
{
    public static BenchmarkPlanValidator DefaultBenchmarkPlanValidator()
    {
        return new BenchmarkPlanValidator(
            new List<IBenchmarkPlanRule>
            {
                new BenchmarkPlanThresholdRangeRule(4.9, 3737.8),
                new BenchmarkPlanCodeLengthRule(1000),
                new BenchmarkPlanRequiredLabelRule("benchmark plan needs a value"),
                new BenchmarkPlanRequiredJobCodeRule("benchmark plan needs this field"),
                new BenchmarkPlanRequiredCodeRule("benchmark plan needs this field"),
            });
    }
}
