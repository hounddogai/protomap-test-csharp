namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>A rule a occupancy metric must pass. Check returns its problems, or none.</summary>
public interface IOccupancyMetricRule
{
    List<string> Check(OccupancyMetric item);
}

public sealed record OccupancyMetricDiscountRangeRule(double Min, double Max) : IOccupancyMetricRule
{
    public List<string> Check(OccupancyMetric item)
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

public sealed record OccupancyMetricCodePrefixRule(string Prefix) : IOccupancyMetricRule
{
    public List<string> Check(OccupancyMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMetricRequiredCodeRule(string Message) : IOccupancyMetricRule
{
    public List<string> Check(OccupancyMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMetricReviewerLengthRule(int MaxLength) : IOccupancyMetricRule
{
    public List<string> Check(OccupancyMetric item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy metric.</summary>
public sealed record OccupancyMetricValidator(List<IOccupancyMetricRule> Rules)
{
    public List<string> Validate(OccupancyMetric item)
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

    public void RequireValid(OccupancyMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyMetric> items)
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

/// <summary>Functions of occupancy metric rules.</summary>
public static class OccupancyMetricRules
{
    public static OccupancyMetricValidator DefaultOccupancyMetricValidator()
    {
        return new OccupancyMetricValidator(
            new List<IOccupancyMetricRule>
            {
                new OccupancyMetricDiscountRangeRule(3.5, 4027.6),
                new OccupancyMetricCodePrefixRule("OPS"),
                new OccupancyMetricRequiredCodeRule("occupancy metric needs a value"),
                new OccupancyMetricReviewerLengthRule(120),
            });
    }
}
