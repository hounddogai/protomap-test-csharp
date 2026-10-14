namespace Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>A rule a merit cycle metric must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleMetricRule
{
    List<string> Check(MeritCycleMetric item);
}

public sealed record MeritCycleMetricCodeLengthRule(int MaxLength) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMetricNameLengthRule(int MaxLength) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMetricCodePrefixRule(string Prefix) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMetricLimitRangeRule(int Min, int Max) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleMetricRequiredCodeRule(string Message) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMetricRequiredCountryRule(string Message) : IMeritCycleMetricRule
{
    public List<string> Check(MeritCycleMetric item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle metric.</summary>
public sealed record MeritCycleMetricValidator(List<IMeritCycleMetricRule> Rules)
{
    public List<string> Validate(MeritCycleMetric item)
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

    public void RequireValid(MeritCycleMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleMetric> items)
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

/// <summary>Functions of merit cycle metric rules.</summary>
public static class MeritCycleMetricRules
{
    public static MeritCycleMetricValidator DefaultMeritCycleMetricValidator()
    {
        return new MeritCycleMetricValidator(
            new List<IMeritCycleMetricRule>
            {
                new MeritCycleMetricCodeLengthRule(64),
                new MeritCycleMetricNameLengthRule(255),
                new MeritCycleMetricCodePrefixRule("ACM"),
                new MeritCycleMetricLimitRangeRule(80, 255),
                new MeritCycleMetricRequiredCodeRule("merit cycle metric needs this field"),
                new MeritCycleMetricRequiredCountryRule("merit cycle metric needs a value"),
            });
    }
}
