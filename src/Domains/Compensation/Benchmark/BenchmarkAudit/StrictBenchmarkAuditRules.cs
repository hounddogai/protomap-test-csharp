namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>A rule a strict benchmark audit must pass. Check returns its problems, or none.</summary>
public interface IStrictBenchmarkAuditRule
{
    List<string> Check(StrictBenchmarkAudit item);
}

public sealed record StrictBenchmarkAuditRequiredLabelRule(string Message) : IStrictBenchmarkAuditRule
{
    public List<string> Check(StrictBenchmarkAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictBenchmarkAuditRateRangeRule(double Min, double Max) : IStrictBenchmarkAuditRule
{
    public List<string> Check(StrictBenchmarkAudit item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictBenchmarkAuditCodeLengthRule(int MaxLength) : IStrictBenchmarkAuditRule
{
    public List<string> Check(StrictBenchmarkAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBenchmarkAuditPriorityRangeRule(int Min, int Max) : IStrictBenchmarkAuditRule
{
    public List<string> Check(StrictBenchmarkAudit item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict benchmark audit.</summary>
public sealed record StrictBenchmarkAuditValidator(List<IStrictBenchmarkAuditRule> Rules)
{
    public List<string> Validate(StrictBenchmarkAudit item)
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

    public void RequireValid(StrictBenchmarkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictBenchmarkAudit> items)
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

/// <summary>Functions of strict benchmark audit rules.</summary>
public static class StrictBenchmarkAuditRules
{
    public static StrictBenchmarkAuditValidator DefaultStrictBenchmarkAuditValidator()
    {
        return new StrictBenchmarkAuditValidator(
            new List<IStrictBenchmarkAuditRule>
            {
                new StrictBenchmarkAuditRequiredLabelRule("strict benchmark audit needs this field"),
                new StrictBenchmarkAuditRateRangeRule(4.1, 4019.7),
                new StrictBenchmarkAuditCodeLengthRule(255),
                new StrictBenchmarkAuditPriorityRangeRule(16, 3600),
            });
    }
}
