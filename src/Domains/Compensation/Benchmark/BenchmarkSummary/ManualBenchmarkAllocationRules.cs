namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>A rule a manual benchmark allocation must pass. Check returns its problems, or none.</summary>
public interface IManualBenchmarkAllocationRule
{
    List<string> Check(ManualBenchmarkAllocation item);
}

public sealed record ManualBenchmarkAllocationRequiredLocationRule(string Message) : IManualBenchmarkAllocationRule
{
    public List<string> Check(ManualBenchmarkAllocation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkAllocationCodeLengthRule(int MaxLength) : IManualBenchmarkAllocationRule
{
    public List<string> Check(ManualBenchmarkAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkAllocationCostRangeRule(double Min, double Max) : IManualBenchmarkAllocationRule
{
    public List<string> Check(ManualBenchmarkAllocation item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual benchmark allocation.</summary>
public sealed record ManualBenchmarkAllocationValidator(List<IManualBenchmarkAllocationRule> Rules)
{
    public List<string> Validate(ManualBenchmarkAllocation item)
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

    public void RequireValid(ManualBenchmarkAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBenchmarkAllocation> items)
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

/// <summary>Functions of manual benchmark allocation rules.</summary>
public static class ManualBenchmarkAllocationRules
{
    public static ManualBenchmarkAllocationValidator DefaultManualBenchmarkAllocationValidator()
    {
        return new ManualBenchmarkAllocationValidator(
            new List<IManualBenchmarkAllocationRule>
            {
                new ManualBenchmarkAllocationRequiredLocationRule("manual benchmark allocation needs this field"),
                new ManualBenchmarkAllocationCodeLengthRule(120),
                new ManualBenchmarkAllocationCostRangeRule(5.7, 1281.4),
            });
    }
}
