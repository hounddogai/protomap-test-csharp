namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate metric must pass. Check returns its problems, or none.</summary>
public interface IRateMetricRule
{
    List<string> Check(RateMetric item);
}

public sealed record RateMetricQuantityRangeRule(int Min, int Max) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateMetricChannelLengthRule(int MaxLength) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateMetricRequiredNameRule(string Message) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateMetricRequiredDescriptionRule(string Message) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateMetricCodeLengthRule(int MaxLength) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateMetricAllowedDescriptionRule(List<string> Allowed) : IRateMetricRule
{
    public List<string> Check(RateMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate metric.</summary>
public sealed record RateMetricValidator(List<IRateMetricRule> Rules)
{
    public List<string> Validate(RateMetric item)
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

    public void RequireValid(RateMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateMetric> items)
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

/// <summary>Functions of rate metric rules.</summary>
public static class RateMetricRules
{
    public static RateMetricValidator DefaultRateMetricValidator()
    {
        return new RateMetricValidator(
            new List<IRateMetricRule>
            {
                new RateMetricQuantityRangeRule(1000, 86400),
                new RateMetricChannelLengthRule(64),
                new RateMetricRequiredNameRule("rate metric needs a value"),
                new RateMetricRequiredDescriptionRule("rate metric needs an entry"),
                new RateMetricCodeLengthRule(80),
                new RateMetricAllowedDescriptionRule(new List<string> { "lenient", "fallback", "global" }),
            });
    }
}
