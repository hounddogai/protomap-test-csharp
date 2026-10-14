namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered benchmark score must pass. Check returns its problems, or none.</summary>
public interface ITieredBenchmarkScoreRule
{
    List<string> Check(TieredBenchmarkScore item);
}

public sealed record TieredBenchmarkScoreAllowedNameRule(List<string> Allowed) : ITieredBenchmarkScoreRule
{
    public List<string> Check(TieredBenchmarkScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkScoreHoursRangeRule(double Min, double Max) : ITieredBenchmarkScoreRule
{
    public List<string> Check(TieredBenchmarkScore item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBenchmarkScoreDiscountRangeRule(double Min, double Max) : ITieredBenchmarkScoreRule
{
    public List<string> Check(TieredBenchmarkScore item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered benchmark score.</summary>
public sealed record TieredBenchmarkScoreValidator(List<ITieredBenchmarkScoreRule> Rules)
{
    public List<string> Validate(TieredBenchmarkScore item)
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

    public void RequireValid(TieredBenchmarkScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBenchmarkScore> items)
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

/// <summary>Functions of tiered benchmark score rules.</summary>
public static class TieredBenchmarkScoreRules
{
    public static TieredBenchmarkScoreValidator DefaultTieredBenchmarkScoreValidator()
    {
        return new TieredBenchmarkScoreValidator(
            new List<ITieredBenchmarkScoreRule>
            {
                new TieredBenchmarkScoreAllowedNameRule(new List<string> { "pending" }),
                new TieredBenchmarkScoreHoursRangeRule(8.4, 3661.9),
                new TieredBenchmarkScoreDiscountRangeRule(1.7, 2929.4),
            });
    }
}
