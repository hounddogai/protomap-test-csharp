namespace Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>A rule a batched payslip metric must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayslipMetricRule
{
    List<string> Check(BatchedPayslipMetric item);
}

public sealed record BatchedPayslipMetricFloorValueRangeRule(double Min, double Max) : IBatchedPayslipMetricRule
{
    public List<string> Check(BatchedPayslipMetric item)
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

public sealed record BatchedPayslipMetricUnitLengthRule(int MaxLength) : IBatchedPayslipMetricRule
{
    public List<string> Check(BatchedPayslipMetric item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayslipMetricRequiredCodeRule(string Message) : IBatchedPayslipMetricRule
{
    public List<string> Check(BatchedPayslipMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched payslip metric.</summary>
public sealed record BatchedPayslipMetricValidator(List<IBatchedPayslipMetricRule> Rules)
{
    public List<string> Validate(BatchedPayslipMetric item)
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

    public void RequireValid(BatchedPayslipMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayslipMetric> items)
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

/// <summary>Functions of batched payslip metric rules.</summary>
public static class BatchedPayslipMetricRules
{
    public static BatchedPayslipMetricValidator DefaultBatchedPayslipMetricValidator()
    {
        return new BatchedPayslipMetricValidator(
            new List<IBatchedPayslipMetricRule>
            {
                new BatchedPayslipMetricFloorValueRangeRule(6.9, 593.2),
                new BatchedPayslipMetricUnitLengthRule(1000),
                new BatchedPayslipMetricRequiredCodeRule("batched payslip metric needs an entry"),
            });
    }
}
