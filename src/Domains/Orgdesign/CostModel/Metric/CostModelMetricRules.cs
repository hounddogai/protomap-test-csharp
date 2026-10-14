namespace Acme.Workforce.Orgdesign.CostModel.Metric;

/// <summary>A rule a cost model metric must pass. Check returns its problems, or none.</summary>
public interface ICostModelMetricRule
{
    List<string> Check(CostModelMetric item);
}

public sealed record CostModelMetricBatchSizeRangeRule(int Min, int Max) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMetricLocaleLengthRule(int MaxLength) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelMetricRequiredContactEmailRule(string Message) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMetricRequiredLocaleRule(string Message) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMetricRequiredCodeRule(string Message) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelMetricFloorValueRangeRule(double Min, double Max) : ICostModelMetricRule
{
    public List<string> Check(CostModelMetric item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a cost model metric.</summary>
public sealed record CostModelMetricValidator(List<ICostModelMetricRule> Rules)
{
    public List<string> Validate(CostModelMetric item)
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

    public void RequireValid(CostModelMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelMetric> items)
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

/// <summary>Functions of cost model metric rules.</summary>
public static class CostModelMetricRules
{
    public static CostModelMetricValidator DefaultCostModelMetricValidator()
    {
        return new CostModelMetricValidator(
            new List<ICostModelMetricRule>
            {
                new CostModelMetricBatchSizeRangeRule(16, 3600),
                new CostModelMetricLocaleLengthRule(16),
                new CostModelMetricRequiredContactEmailRule("cost model metric needs this field"),
                new CostModelMetricRequiredLocaleRule("cost model metric needs an entry"),
                new CostModelMetricRequiredCodeRule("cost model metric needs this field"),
                new CostModelMetricFloorValueRangeRule(8.5, 157.8),
            });
    }
}
