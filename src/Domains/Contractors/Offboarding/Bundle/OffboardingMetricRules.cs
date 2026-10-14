namespace Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>A rule a offboarding metric must pass. Check returns its problems, or none.</summary>
public interface IOffboardingMetricRule
{
    List<string> Check(OffboardingMetric item);
}

public sealed record OffboardingMetricRequiredNameRule(string Message) : IOffboardingMetricRule
{
    public List<string> Check(OffboardingMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMetricCodeLengthRule(int MaxLength) : IOffboardingMetricRule
{
    public List<string> Check(OffboardingMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMetricRateRangeRule(double Min, double Max) : IOffboardingMetricRule
{
    public List<string> Check(OffboardingMetric item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMetricNameLengthRule(int MaxLength) : IOffboardingMetricRule
{
    public List<string> Check(OffboardingMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingMetricLimitRangeRule(int Min, int Max) : IOffboardingMetricRule
{
    public List<string> Check(OffboardingMetric item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding metric.</summary>
public sealed record OffboardingMetricValidator(List<IOffboardingMetricRule> Rules)
{
    public List<string> Validate(OffboardingMetric item)
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

    public void RequireValid(OffboardingMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingMetric> items)
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

/// <summary>Functions of offboarding metric rules.</summary>
public static class OffboardingMetricRules
{
    public static OffboardingMetricValidator DefaultOffboardingMetricValidator()
    {
        return new OffboardingMetricValidator(
            new List<IOffboardingMetricRule>
            {
                new OffboardingMetricRequiredNameRule("offboarding metric needs a value"),
                new OffboardingMetricCodeLengthRule(32),
                new OffboardingMetricRateRangeRule(5.0, 3819.7),
                new OffboardingMetricNameLengthRule(80),
                new OffboardingMetricLimitRangeRule(16, 3600),
            });
    }
}
