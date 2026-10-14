namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark forecast must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkForecastRule
{
    List<string> Check(BenchmarkForecast item);
}

public sealed record BenchmarkForecastSequenceRangeRule(int Min, int Max) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkForecastTenureMonthsRangeRule(int Min, int Max) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkForecastAllowedCodeRule(List<string> Allowed) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkForecastRequiredCodeRule(string Message) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkForecastRatioRangeRule(double Min, double Max) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkForecastLabelLengthRule(int MaxLength) : IBenchmarkForecastRule
{
    public List<string> Check(BenchmarkForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark forecast.</summary>
public sealed record BenchmarkForecastValidator(List<IBenchmarkForecastRule> Rules)
{
    public List<string> Validate(BenchmarkForecast item)
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

    public void RequireValid(BenchmarkForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkForecast> items)
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

/// <summary>Functions of benchmark forecast rules.</summary>
public static class BenchmarkForecastRules
{
    public static BenchmarkForecastValidator DefaultBenchmarkForecastValidator()
    {
        return new BenchmarkForecastValidator(
            new List<IBenchmarkForecastRule>
            {
                new BenchmarkForecastSequenceRangeRule(80, 86400),
                new BenchmarkForecastTenureMonthsRangeRule(32, 86400),
                new BenchmarkForecastAllowedCodeRule(new List<string> { "primary", "capped", "seasonal" }),
                new BenchmarkForecastRequiredCodeRule("benchmark forecast needs a value"),
                new BenchmarkForecastRatioRangeRule(8.4, 65.5),
                new BenchmarkForecastLabelLengthRule(3600),
            });
    }
}
