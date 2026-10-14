namespace Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>A rule a benchmark audit must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkAuditRule
{
    List<string> Check(BenchmarkAudit item);
}

public sealed record BenchmarkAuditOwnerIdLengthRule(int MaxLength) : IBenchmarkAuditRule
{
    public List<string> Check(BenchmarkAudit item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAuditRequiredCodeRule(string Message) : IBenchmarkAuditRule
{
    public List<string> Check(BenchmarkAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAuditRatioRangeRule(double Min, double Max) : IBenchmarkAuditRule
{
    public List<string> Check(BenchmarkAudit item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAuditCodePrefixRule(string Prefix) : IBenchmarkAuditRule
{
    public List<string> Check(BenchmarkAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAuditGraceDaysRangeRule(int Min, int Max) : IBenchmarkAuditRule
{
    public List<string> Check(BenchmarkAudit item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark audit.</summary>
public sealed record BenchmarkAuditValidator(List<IBenchmarkAuditRule> Rules)
{
    public List<string> Validate(BenchmarkAudit item)
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

    public void RequireValid(BenchmarkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkAudit> items)
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

/// <summary>Functions of benchmark audit rules.</summary>
public static class BenchmarkAuditRules
{
    public static BenchmarkAuditValidator DefaultBenchmarkAuditValidator()
    {
        return new BenchmarkAuditValidator(
            new List<IBenchmarkAuditRule>
            {
                new BenchmarkAuditOwnerIdLengthRule(16),
                new BenchmarkAuditRequiredCodeRule("benchmark audit needs this field"),
                new BenchmarkAuditRatioRangeRule(0.4, 1378.2),
                new BenchmarkAuditCodePrefixRule("Q"),
                new BenchmarkAuditGraceDaysRangeRule(16, 32),
            });
    }
}
