namespace Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>A rule a benchmark baseline must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkBaselineRule
{
    List<string> Check(BenchmarkBaseline item);
}

public sealed record BenchmarkBaselineSkillLengthRule(int MaxLength) : IBenchmarkBaselineRule
{
    public List<string> Check(BenchmarkBaseline item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBaselineRequiredTeamRule(string Message) : IBenchmarkBaselineRule
{
    public List<string> Check(BenchmarkBaseline item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkBaselineRateRangeRule(double Min, double Max) : IBenchmarkBaselineRule
{
    public List<string> Check(BenchmarkBaseline item)
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

public sealed record BenchmarkBaselineCapacityRangeRule(int Min, int Max) : IBenchmarkBaselineRule
{
    public List<string> Check(BenchmarkBaseline item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkBaselineCodePrefixRule(string Prefix) : IBenchmarkBaselineRule
{
    public List<string> Check(BenchmarkBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark baseline.</summary>
public sealed record BenchmarkBaselineValidator(List<IBenchmarkBaselineRule> Rules)
{
    public List<string> Validate(BenchmarkBaseline item)
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

    public void RequireValid(BenchmarkBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkBaseline> items)
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

/// <summary>Functions of benchmark baseline rules.</summary>
public static class BenchmarkBaselineRules
{
    public static BenchmarkBaselineValidator DefaultBenchmarkBaselineValidator()
    {
        return new BenchmarkBaselineValidator(
            new List<IBenchmarkBaselineRule>
            {
                new BenchmarkBaselineSkillLengthRule(1000),
                new BenchmarkBaselineRequiredTeamRule("benchmark baseline needs this field"),
                new BenchmarkBaselineRateRangeRule(7.0, 3036.7),
                new BenchmarkBaselineCapacityRangeRule(80, 255),
                new BenchmarkBaselineCodePrefixRule("REF"),
            });
    }
}
