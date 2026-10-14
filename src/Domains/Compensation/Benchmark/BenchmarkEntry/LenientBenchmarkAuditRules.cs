namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>A rule a lenient benchmark audit must pass. Check returns its problems, or none.</summary>
public interface ILenientBenchmarkAuditRule
{
    List<string> Check(LenientBenchmarkAudit item);
}

public sealed record LenientBenchmarkAuditRequiredManagerIdRule(string Message) : ILenientBenchmarkAuditRule
{
    public List<string> Check(LenientBenchmarkAudit item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkAuditSequenceRangeRule(int Min, int Max) : ILenientBenchmarkAuditRule
{
    public List<string> Check(LenientBenchmarkAudit item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBenchmarkAuditMinimumRangeRule(double Min, double Max) : ILenientBenchmarkAuditRule
{
    public List<string> Check(LenientBenchmarkAudit item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient benchmark audit.</summary>
public sealed record LenientBenchmarkAuditValidator(List<ILenientBenchmarkAuditRule> Rules)
{
    public List<string> Validate(LenientBenchmarkAudit item)
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

    public void RequireValid(LenientBenchmarkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBenchmarkAudit> items)
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

/// <summary>Functions of lenient benchmark audit rules.</summary>
public static class LenientBenchmarkAuditRules
{
    public static LenientBenchmarkAuditValidator DefaultLenientBenchmarkAuditValidator()
    {
        return new LenientBenchmarkAuditValidator(
            new List<ILenientBenchmarkAuditRule>
            {
                new LenientBenchmarkAuditRequiredManagerIdRule("lenient benchmark audit needs an entry"),
                new LenientBenchmarkAuditSequenceRangeRule(80, 3600),
                new LenientBenchmarkAuditMinimumRangeRule(9.5, 4297.7),
            });
    }
}
