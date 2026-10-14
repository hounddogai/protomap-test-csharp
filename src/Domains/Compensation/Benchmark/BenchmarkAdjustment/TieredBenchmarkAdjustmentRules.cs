namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>A rule a tiered benchmark adjustment must pass. Check returns its problems, or none.</summary>
public interface ITieredBenchmarkAdjustmentRule
{
    List<string> Check(TieredBenchmarkAdjustment item);
}

public sealed record TieredBenchmarkAdjustmentNameLengthRule(int MaxLength) : ITieredBenchmarkAdjustmentRule
{
    public List<string> Check(TieredBenchmarkAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkAdjustmentCodeLengthRule(int MaxLength) : ITieredBenchmarkAdjustmentRule
{
    public List<string> Check(TieredBenchmarkAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkAdjustmentNotesLengthRule(int MaxLength) : ITieredBenchmarkAdjustmentRule
{
    public List<string> Check(TieredBenchmarkAdjustment item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkAdjustmentCodePrefixRule(string Prefix) : ITieredBenchmarkAdjustmentRule
{
    public List<string> Check(TieredBenchmarkAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered benchmark adjustment.</summary>
public sealed record TieredBenchmarkAdjustmentValidator(List<ITieredBenchmarkAdjustmentRule> Rules)
{
    public List<string> Validate(TieredBenchmarkAdjustment item)
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

    public void RequireValid(TieredBenchmarkAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBenchmarkAdjustment> items)
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

/// <summary>Functions of tiered benchmark adjustment rules.</summary>
public static class TieredBenchmarkAdjustmentRules
{
    public static TieredBenchmarkAdjustmentValidator DefaultTieredBenchmarkAdjustmentValidator()
    {
        return new TieredBenchmarkAdjustmentValidator(
            new List<ITieredBenchmarkAdjustmentRule>
            {
                new TieredBenchmarkAdjustmentNameLengthRule(16),
                new TieredBenchmarkAdjustmentCodeLengthRule(120),
                new TieredBenchmarkAdjustmentNotesLengthRule(1000),
                new TieredBenchmarkAdjustmentCodePrefixRule("OPS"),
            });
    }
}
