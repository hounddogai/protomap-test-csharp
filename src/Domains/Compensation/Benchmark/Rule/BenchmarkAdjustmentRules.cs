namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark adjustment must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkAdjustmentRule
{
    List<string> Check(BenchmarkAdjustment item);
}

public sealed record BenchmarkAdjustmentCodeLengthRule(int MaxLength) : IBenchmarkAdjustmentRule
{
    public List<string> Check(BenchmarkAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAdjustmentAllowedExternalRefRule(List<string> Allowed) : IBenchmarkAdjustmentRule
{
    public List<string> Check(BenchmarkAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAdjustmentRequiredCodeRule(string Message) : IBenchmarkAdjustmentRule
{
    public List<string> Check(BenchmarkAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark adjustment.</summary>
public sealed record BenchmarkAdjustmentValidator(List<IBenchmarkAdjustmentRule> Rules)
{
    public List<string> Validate(BenchmarkAdjustment item)
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

    public void RequireValid(BenchmarkAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkAdjustment> items)
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

/// <summary>Functions of benchmark adjustment rules.</summary>
public static class BenchmarkAdjustmentRules
{
    public static BenchmarkAdjustmentValidator DefaultBenchmarkAdjustmentValidator()
    {
        return new BenchmarkAdjustmentValidator(
            new List<IBenchmarkAdjustmentRule>
            {
                new BenchmarkAdjustmentCodeLengthRule(86400),
                new BenchmarkAdjustmentAllowedExternalRefRule(new List<string> { "weighted" }),
                new BenchmarkAdjustmentRequiredCodeRule("benchmark adjustment needs this field"),
            });
    }
}
