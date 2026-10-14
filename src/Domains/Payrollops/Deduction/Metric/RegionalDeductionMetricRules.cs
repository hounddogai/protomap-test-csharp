namespace Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>A rule a regional deduction metric must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeductionMetricRule
{
    List<string> Check(RegionalDeductionMetric item);
}

public sealed record RegionalDeductionMetricLabelLengthRule(int MaxLength) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionMetricWeightRangeRule(double Min, double Max) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalDeductionMetricDepartmentLengthRule(int MaxLength) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionMetricMarginRangeRule(double Min, double Max) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalDeductionMetricRequiredDepartmentRule(string Message) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeductionMetricRequiredCodeRule(string Message) : IRegionalDeductionMetricRule
{
    public List<string> Check(RegionalDeductionMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional deduction metric.</summary>
public sealed record RegionalDeductionMetricValidator(List<IRegionalDeductionMetricRule> Rules)
{
    public List<string> Validate(RegionalDeductionMetric item)
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

    public void RequireValid(RegionalDeductionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeductionMetric> items)
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

/// <summary>Functions of regional deduction metric rules.</summary>
public static class RegionalDeductionMetricRules
{
    public static RegionalDeductionMetricValidator DefaultRegionalDeductionMetricValidator()
    {
        return new RegionalDeductionMetricValidator(
            new List<IRegionalDeductionMetricRule>
            {
                new RegionalDeductionMetricLabelLengthRule(120),
                new RegionalDeductionMetricWeightRangeRule(4.9, 3685.6),
                new RegionalDeductionMetricDepartmentLengthRule(32),
                new RegionalDeductionMetricMarginRangeRule(8.0, 608.6),
                new RegionalDeductionMetricRequiredDepartmentRule("regional deduction metric needs a value"),
                new RegionalDeductionMetricRequiredCodeRule("regional deduction metric needs this field"),
            });
    }
}
