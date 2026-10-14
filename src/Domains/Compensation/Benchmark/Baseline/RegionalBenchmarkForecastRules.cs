namespace Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>A rule a regional benchmark forecast must pass. Check returns its problems, or none.</summary>
public interface IRegionalBenchmarkForecastRule
{
    List<string> Check(RegionalBenchmarkForecast item);
}

public sealed record RegionalBenchmarkForecastRequiredNameRule(string Message) : IRegionalBenchmarkForecastRule
{
    public List<string> Check(RegionalBenchmarkForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkForecastCodeLengthRule(int MaxLength) : IRegionalBenchmarkForecastRule
{
    public List<string> Check(RegionalBenchmarkForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkForecastRequiredBadgeRule(string Message) : IRegionalBenchmarkForecastRule
{
    public List<string> Check(RegionalBenchmarkForecast item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBenchmarkForecastTeamLengthRule(int MaxLength) : IRegionalBenchmarkForecastRule
{
    public List<string> Check(RegionalBenchmarkForecast item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional benchmark forecast.</summary>
public sealed record RegionalBenchmarkForecastValidator(List<IRegionalBenchmarkForecastRule> Rules)
{
    public List<string> Validate(RegionalBenchmarkForecast item)
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

    public void RequireValid(RegionalBenchmarkForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBenchmarkForecast> items)
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

/// <summary>Functions of regional benchmark forecast rules.</summary>
public static class RegionalBenchmarkForecastRules
{
    public static RegionalBenchmarkForecastValidator DefaultRegionalBenchmarkForecastValidator()
    {
        return new RegionalBenchmarkForecastValidator(
            new List<IRegionalBenchmarkForecastRule>
            {
                new RegionalBenchmarkForecastRequiredNameRule("regional benchmark forecast needs an entry"),
                new RegionalBenchmarkForecastCodeLengthRule(3600),
                new RegionalBenchmarkForecastRequiredBadgeRule("regional benchmark forecast needs this field"),
                new RegionalBenchmarkForecastTeamLengthRule(3600),
            });
    }
}
