namespace Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>A rule a benchmark mapping must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkMappingRule
{
    List<string> Check(BenchmarkMapping item);
}

public sealed record BenchmarkMappingCodeLengthRule(int MaxLength) : IBenchmarkMappingRule
{
    public List<string> Check(BenchmarkMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMappingTitleLengthRule(int MaxLength) : IBenchmarkMappingRule
{
    public List<string> Check(BenchmarkMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMappingRequiredNotesRule(string Message) : IBenchmarkMappingRule
{
    public List<string> Check(BenchmarkMapping item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark mapping.</summary>
public sealed record BenchmarkMappingValidator(List<IBenchmarkMappingRule> Rules)
{
    public List<string> Validate(BenchmarkMapping item)
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

    public void RequireValid(BenchmarkMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkMapping> items)
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

/// <summary>Functions of benchmark mapping rules.</summary>
public static class BenchmarkMappingRules
{
    public static BenchmarkMappingValidator DefaultBenchmarkMappingValidator()
    {
        return new BenchmarkMappingValidator(
            new List<IBenchmarkMappingRule>
            {
                new BenchmarkMappingCodeLengthRule(32),
                new BenchmarkMappingTitleLengthRule(255),
                new BenchmarkMappingRequiredNotesRule("benchmark mapping needs an entry"),
            });
    }
}
