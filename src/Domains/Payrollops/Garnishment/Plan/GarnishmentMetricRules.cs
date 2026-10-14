namespace Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>A rule a garnishment metric must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentMetricRule
{
    List<string> Check(GarnishmentMetric item);
}

public sealed record GarnishmentMetricSequenceRangeRule(int Min, int Max) : IGarnishmentMetricRule
{
    public List<string> Check(GarnishmentMetric item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentMetricRequiredLocaleRule(string Message) : IGarnishmentMetricRule
{
    public List<string> Check(GarnishmentMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMetricCountryLengthRule(int MaxLength) : IGarnishmentMetricRule
{
    public List<string> Check(GarnishmentMetric item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMetricCodePrefixRule(string Prefix) : IGarnishmentMetricRule
{
    public List<string> Check(GarnishmentMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMetricCodeLengthRule(int MaxLength) : IGarnishmentMetricRule
{
    public List<string> Check(GarnishmentMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment metric.</summary>
public sealed record GarnishmentMetricValidator(List<IGarnishmentMetricRule> Rules)
{
    public List<string> Validate(GarnishmentMetric item)
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

    public void RequireValid(GarnishmentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentMetric> items)
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

/// <summary>Functions of garnishment metric rules.</summary>
public static class GarnishmentMetricRules
{
    public static GarnishmentMetricValidator DefaultGarnishmentMetricValidator()
    {
        return new GarnishmentMetricValidator(
            new List<IGarnishmentMetricRule>
            {
                new GarnishmentMetricSequenceRangeRule(32, 80),
                new GarnishmentMetricRequiredLocaleRule("garnishment metric needs a value"),
                new GarnishmentMetricCountryLengthRule(1000),
                new GarnishmentMetricCodePrefixRule("ACM"),
                new GarnishmentMetricCodeLengthRule(64),
            });
    }
}
