namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>A rule a seasonal benchmark digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBenchmarkDigestRule
{
    List<string> Check(SeasonalBenchmarkDigest item);
}

public sealed record SeasonalBenchmarkDigestRequiredBadgeRule(string Message) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBenchmarkDigestRequiredLabelRule(string Message) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBenchmarkDigestCodePrefixRule(string Prefix) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBenchmarkDigestRateRangeRule(double Min, double Max) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBenchmarkDigestTimezoneLengthRule(int MaxLength) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBenchmarkDigestCodeLengthRule(int MaxLength) : ISeasonalBenchmarkDigestRule
{
    public List<string> Check(SeasonalBenchmarkDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal benchmark digest.</summary>
public sealed record SeasonalBenchmarkDigestValidator(List<ISeasonalBenchmarkDigestRule> Rules)
{
    public List<string> Validate(SeasonalBenchmarkDigest item)
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

    public void RequireValid(SeasonalBenchmarkDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBenchmarkDigest> items)
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

/// <summary>Functions of seasonal benchmark digest rules.</summary>
public static class SeasonalBenchmarkDigestRules
{
    public static SeasonalBenchmarkDigestValidator DefaultSeasonalBenchmarkDigestValidator()
    {
        return new SeasonalBenchmarkDigestValidator(
            new List<ISeasonalBenchmarkDigestRule>
            {
                new SeasonalBenchmarkDigestRequiredBadgeRule("seasonal benchmark digest needs an entry"),
                new SeasonalBenchmarkDigestRequiredLabelRule("seasonal benchmark digest needs an entry"),
                new SeasonalBenchmarkDigestCodePrefixRule("X"),
                new SeasonalBenchmarkDigestRateRangeRule(1.3, 1609.6),
                new SeasonalBenchmarkDigestTimezoneLengthRule(120),
                new SeasonalBenchmarkDigestCodeLengthRule(16),
            });
    }
}
