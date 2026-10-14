namespace Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>A rule a benchmark preference must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkPreferenceRule
{
    List<string> Check(BenchmarkPreference item);
}

public sealed record BenchmarkPreferenceRequiredCodeRule(string Message) : IBenchmarkPreferenceRule
{
    public List<string> Check(BenchmarkPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPreferenceTitleLengthRule(int MaxLength) : IBenchmarkPreferenceRule
{
    public List<string> Check(BenchmarkPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPreferenceRequiredTitleRule(string Message) : IBenchmarkPreferenceRule
{
    public List<string> Check(BenchmarkPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPreferenceTenureMonthsRangeRule(int Min, int Max) : IBenchmarkPreferenceRule
{
    public List<string> Check(BenchmarkPreference item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark preference.</summary>
public sealed record BenchmarkPreferenceValidator(List<IBenchmarkPreferenceRule> Rules)
{
    public List<string> Validate(BenchmarkPreference item)
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

    public void RequireValid(BenchmarkPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkPreference> items)
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

/// <summary>Functions of benchmark preference rules.</summary>
public static class BenchmarkPreferenceRules
{
    public static BenchmarkPreferenceValidator DefaultBenchmarkPreferenceValidator()
    {
        return new BenchmarkPreferenceValidator(
            new List<IBenchmarkPreferenceRule>
            {
                new BenchmarkPreferenceRequiredCodeRule("benchmark preference needs this field"),
                new BenchmarkPreferenceTitleLengthRule(3600),
                new BenchmarkPreferenceRequiredTitleRule("benchmark preference needs an entry"),
                new BenchmarkPreferenceTenureMonthsRangeRule(80, 86400),
            });
    }
}
