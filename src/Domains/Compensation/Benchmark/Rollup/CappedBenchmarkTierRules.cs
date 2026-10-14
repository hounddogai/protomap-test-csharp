namespace Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>A rule a capped benchmark tier must pass. Check returns its problems, or none.</summary>
public interface ICappedBenchmarkTierRule
{
    List<string> Check(CappedBenchmarkTier item);
}

public sealed record CappedBenchmarkTierLabelLengthRule(int MaxLength) : ICappedBenchmarkTierRule
{
    public List<string> Check(CappedBenchmarkTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkTierHoursRangeRule(double Min, double Max) : ICappedBenchmarkTierRule
{
    public List<string> Check(CappedBenchmarkTier item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedBenchmarkTierCodePrefixRule(string Prefix) : ICappedBenchmarkTierRule
{
    public List<string> Check(CappedBenchmarkTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkTierCodeLengthRule(int MaxLength) : ICappedBenchmarkTierRule
{
    public List<string> Check(CappedBenchmarkTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkTierRequiredCodeRule(string Message) : ICappedBenchmarkTierRule
{
    public List<string> Check(CappedBenchmarkTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped benchmark tier.</summary>
public sealed record CappedBenchmarkTierValidator(List<ICappedBenchmarkTierRule> Rules)
{
    public List<string> Validate(CappedBenchmarkTier item)
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

    public void RequireValid(CappedBenchmarkTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBenchmarkTier> items)
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

/// <summary>Functions of capped benchmark tier rules.</summary>
public static class CappedBenchmarkTierRules
{
    public static CappedBenchmarkTierValidator DefaultCappedBenchmarkTierValidator()
    {
        return new CappedBenchmarkTierValidator(
            new List<ICappedBenchmarkTierRule>
            {
                new CappedBenchmarkTierLabelLengthRule(32),
                new CappedBenchmarkTierHoursRangeRule(0.4, 4513.4),
                new CappedBenchmarkTierCodePrefixRule("OPS"),
                new CappedBenchmarkTierCodeLengthRule(64),
                new CappedBenchmarkTierRequiredCodeRule("capped benchmark tier needs a value"),
            });
    }
}
