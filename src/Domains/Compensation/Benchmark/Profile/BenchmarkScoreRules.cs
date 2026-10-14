namespace Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>A rule a benchmark score must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkScoreRule
{
    List<string> Check(BenchmarkScore item);
}

public sealed record BenchmarkScoreGradeLengthRule(int MaxLength) : IBenchmarkScoreRule
{
    public List<string> Check(BenchmarkScore item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkScoreRevisionRangeRule(int Min, int Max) : IBenchmarkScoreRule
{
    public List<string> Check(BenchmarkScore item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkScoreRequiredLabelRule(string Message) : IBenchmarkScoreRule
{
    public List<string> Check(BenchmarkScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark score.</summary>
public sealed record BenchmarkScoreValidator(List<IBenchmarkScoreRule> Rules)
{
    public List<string> Validate(BenchmarkScore item)
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

    public void RequireValid(BenchmarkScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkScore> items)
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

/// <summary>Functions of benchmark score rules.</summary>
public static class BenchmarkScoreRules
{
    public static BenchmarkScoreValidator DefaultBenchmarkScoreValidator()
    {
        return new BenchmarkScoreValidator(
            new List<IBenchmarkScoreRule>
            {
                new BenchmarkScoreGradeLengthRule(255),
                new BenchmarkScoreRevisionRangeRule(64, 86400),
                new BenchmarkScoreRequiredLabelRule("benchmark score needs an entry"),
            });
    }
}
