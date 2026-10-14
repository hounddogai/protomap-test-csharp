namespace Acme.Workforce.Assets.Warranty.Metric;

/// <summary>A rule a warranty metric must pass. Check returns its problems, or none.</summary>
public interface IWarrantyMetricRule
{
    List<string> Check(WarrantyMetric item);
}

public sealed record WarrantyMetricRequiredRegionRule(string Message) : IWarrantyMetricRule
{
    public List<string> Check(WarrantyMetric item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMetricRegionLengthRule(int MaxLength) : IWarrantyMetricRule
{
    public List<string> Check(WarrantyMetric item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMetricCreatedAtRangeRule(int Min, int Max) : IWarrantyMetricRule
{
    public List<string> Check(WarrantyMetric item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a warranty metric.</summary>
public sealed record WarrantyMetricValidator(List<IWarrantyMetricRule> Rules)
{
    public List<string> Validate(WarrantyMetric item)
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

    public void RequireValid(WarrantyMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyMetric> items)
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

/// <summary>Functions of warranty metric rules.</summary>
public static class WarrantyMetricRules
{
    public static WarrantyMetricValidator DefaultWarrantyMetricValidator()
    {
        return new WarrantyMetricValidator(
            new List<IWarrantyMetricRule>
            {
                new WarrantyMetricRequiredRegionRule("warranty metric needs this field"),
                new WarrantyMetricRegionLengthRule(3600),
                new WarrantyMetricCreatedAtRangeRule(255, 1000),
            });
    }
}
