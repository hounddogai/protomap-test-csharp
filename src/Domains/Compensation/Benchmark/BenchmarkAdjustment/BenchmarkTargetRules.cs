namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>A rule a benchmark target must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkTargetRule
{
    List<string> Check(BenchmarkTarget item);
}

public sealed record BenchmarkTargetNameLengthRule(int MaxLength) : IBenchmarkTargetRule
{
    public List<string> Check(BenchmarkTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkTargetRequiredNameRule(string Message) : IBenchmarkTargetRule
{
    public List<string> Check(BenchmarkTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkTargetRetriesRangeRule(int Min, int Max) : IBenchmarkTargetRule
{
    public List<string> Check(BenchmarkTarget item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkTargetCodePrefixRule(string Prefix) : IBenchmarkTargetRule
{
    public List<string> Check(BenchmarkTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark target.</summary>
public sealed record BenchmarkTargetValidator(List<IBenchmarkTargetRule> Rules)
{
    public List<string> Validate(BenchmarkTarget item)
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

    public void RequireValid(BenchmarkTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkTarget> items)
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

/// <summary>Functions of benchmark target rules.</summary>
public static class BenchmarkTargetRules
{
    public static BenchmarkTargetValidator DefaultBenchmarkTargetValidator()
    {
        return new BenchmarkTargetValidator(
            new List<IBenchmarkTargetRule>
            {
                new BenchmarkTargetNameLengthRule(64),
                new BenchmarkTargetRequiredNameRule("benchmark target needs this field"),
                new BenchmarkTargetRetriesRangeRule(64, 3600),
                new BenchmarkTargetCodePrefixRule("X"),
            });
    }
}
