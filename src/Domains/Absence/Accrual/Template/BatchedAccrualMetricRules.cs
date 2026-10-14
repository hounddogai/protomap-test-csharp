namespace Acme.Workforce.Absence.Accrual.Template;

/// <summary>A rule a batched accrual metric must pass. Check returns its problems, or none.</summary>
public interface IBatchedAccrualMetricRule
{
    List<string> Check(BatchedAccrualMetric item);
}

public sealed record BatchedAccrualMetricRequiredCodeRule(string Message) : IBatchedAccrualMetricRule
{
    public List<string> Check(BatchedAccrualMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualMetricCodeLengthRule(int MaxLength) : IBatchedAccrualMetricRule
{
    public List<string> Check(BatchedAccrualMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualMetricRequiredDepartmentRule(string Message) : IBatchedAccrualMetricRule
{
    public List<string> Check(BatchedAccrualMetric item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualMetricRequiredLabelRule(string Message) : IBatchedAccrualMetricRule
{
    public List<string> Check(BatchedAccrualMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched accrual metric.</summary>
public sealed record BatchedAccrualMetricValidator(List<IBatchedAccrualMetricRule> Rules)
{
    public List<string> Validate(BatchedAccrualMetric item)
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

    public void RequireValid(BatchedAccrualMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAccrualMetric> items)
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

/// <summary>Functions of batched accrual metric rules.</summary>
public static class BatchedAccrualMetricRules
{
    public static BatchedAccrualMetricValidator DefaultBatchedAccrualMetricValidator()
    {
        return new BatchedAccrualMetricValidator(
            new List<IBatchedAccrualMetricRule>
            {
                new BatchedAccrualMetricRequiredCodeRule("batched accrual metric needs this field"),
                new BatchedAccrualMetricCodeLengthRule(64),
                new BatchedAccrualMetricRequiredDepartmentRule("batched accrual metric needs an entry"),
                new BatchedAccrualMetricRequiredLabelRule("batched accrual metric needs a value"),
            });
    }
}
