namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>A rule a batched garnishment metric must pass. Check returns its problems, or none.</summary>
public interface IBatchedGarnishmentMetricRule
{
    List<string> Check(BatchedGarnishmentMetric item);
}

public sealed record BatchedGarnishmentMetricRetriesRangeRule(int Min, int Max) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedGarnishmentMetricHoursRangeRule(double Min, double Max) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
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

public sealed record BatchedGarnishmentMetricNameLengthRule(int MaxLength) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedGarnishmentMetricRequiredContactEmailRule(string Message) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedGarnishmentMetricCodePrefixRule(string Prefix) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedGarnishmentMetricRequiredCodeRule(string Message) : IBatchedGarnishmentMetricRule
{
    public List<string> Check(BatchedGarnishmentMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched garnishment metric.</summary>
public sealed record BatchedGarnishmentMetricValidator(List<IBatchedGarnishmentMetricRule> Rules)
{
    public List<string> Validate(BatchedGarnishmentMetric item)
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

    public void RequireValid(BatchedGarnishmentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedGarnishmentMetric> items)
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

/// <summary>Functions of batched garnishment metric rules.</summary>
public static class BatchedGarnishmentMetricRules
{
    public static BatchedGarnishmentMetricValidator DefaultBatchedGarnishmentMetricValidator()
    {
        return new BatchedGarnishmentMetricValidator(
            new List<IBatchedGarnishmentMetricRule>
            {
                new BatchedGarnishmentMetricRetriesRangeRule(120, 3600),
                new BatchedGarnishmentMetricHoursRangeRule(9.9, 4154.6),
                new BatchedGarnishmentMetricNameLengthRule(32),
                new BatchedGarnishmentMetricRequiredContactEmailRule("batched garnishment metric needs this field"),
                new BatchedGarnishmentMetricCodePrefixRule("X"),
                new BatchedGarnishmentMetricRequiredCodeRule("batched garnishment metric needs this field"),
            });
    }
}
