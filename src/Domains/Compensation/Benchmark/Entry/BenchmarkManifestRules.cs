namespace Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>A rule a benchmark manifest must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkManifestRule
{
    List<string> Check(BenchmarkManifest item);
}

public sealed record BenchmarkManifestFloorValueRangeRule(double Min, double Max) : IBenchmarkManifestRule
{
    public List<string> Check(BenchmarkManifest item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkManifestLabelLengthRule(int MaxLength) : IBenchmarkManifestRule
{
    public List<string> Check(BenchmarkManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkManifestCodePrefixRule(string Prefix) : IBenchmarkManifestRule
{
    public List<string> Check(BenchmarkManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark manifest.</summary>
public sealed record BenchmarkManifestValidator(List<IBenchmarkManifestRule> Rules)
{
    public List<string> Validate(BenchmarkManifest item)
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

    public void RequireValid(BenchmarkManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkManifest> items)
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

/// <summary>Functions of benchmark manifest rules.</summary>
public static class BenchmarkManifestRules
{
    public static BenchmarkManifestValidator DefaultBenchmarkManifestValidator()
    {
        return new BenchmarkManifestValidator(
            new List<IBenchmarkManifestRule>
            {
                new BenchmarkManifestFloorValueRangeRule(3.1, 4226.7),
                new BenchmarkManifestLabelLengthRule(80),
                new BenchmarkManifestCodePrefixRule("Q"),
            });
    }
}
