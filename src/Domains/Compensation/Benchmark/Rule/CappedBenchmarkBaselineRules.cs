namespace Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>A rule a capped benchmark baseline must pass. Check returns its problems, or none.</summary>
public interface ICappedBenchmarkBaselineRule
{
    List<string> Check(CappedBenchmarkBaseline item);
}

public sealed record CappedBenchmarkBaselineLabelLengthRule(int MaxLength) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkBaselineExternalRefLengthRule(int MaxLength) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkBaselineRequiredJobCodeRule(string Message) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkBaselineHoursRangeRule(double Min, double Max) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkBaselineRequiredExternalRefRule(string Message) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedBenchmarkBaselineCodePrefixRule(string Prefix) : ICappedBenchmarkBaselineRule
{
    public List<string> Check(CappedBenchmarkBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped benchmark baseline.</summary>
public sealed record CappedBenchmarkBaselineValidator(List<ICappedBenchmarkBaselineRule> Rules)
{
    public List<string> Validate(CappedBenchmarkBaseline item)
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

    public void RequireValid(CappedBenchmarkBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBenchmarkBaseline> items)
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

/// <summary>Functions of capped benchmark baseline rules.</summary>
public static class CappedBenchmarkBaselineRules
{
    public static CappedBenchmarkBaselineValidator DefaultCappedBenchmarkBaselineValidator()
    {
        return new CappedBenchmarkBaselineValidator(
            new List<ICappedBenchmarkBaselineRule>
            {
                new CappedBenchmarkBaselineLabelLengthRule(16),
                new CappedBenchmarkBaselineExternalRefLengthRule(3600),
                new CappedBenchmarkBaselineRequiredJobCodeRule("capped benchmark baseline needs this field"),
                new CappedBenchmarkBaselineHoursRangeRule(0.4, 1571.2),
                new CappedBenchmarkBaselineRequiredExternalRefRule("capped benchmark baseline needs this field"),
                new CappedBenchmarkBaselineCodePrefixRule("REF"),
            });
    }
}
