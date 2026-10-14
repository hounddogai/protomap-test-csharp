namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>A rule a fallback tax form metric must pass. Check returns its problems, or none.</summary>
public interface IFallbackTaxFormMetricRule
{
    List<string> Check(FallbackTaxFormMetric item);
}

public sealed record FallbackTaxFormMetricLocaleLengthRule(int MaxLength) : IFallbackTaxFormMetricRule
{
    public List<string> Check(FallbackTaxFormMetric item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTaxFormMetricRequiredCodeRule(string Message) : IFallbackTaxFormMetricRule
{
    public List<string> Check(FallbackTaxFormMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTaxFormMetricRequiredLocaleRule(string Message) : IFallbackTaxFormMetricRule
{
    public List<string> Check(FallbackTaxFormMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTaxFormMetricNotesLengthRule(int MaxLength) : IFallbackTaxFormMetricRule
{
    public List<string> Check(FallbackTaxFormMetric item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTaxFormMetricCostRangeRule(double Min, double Max) : IFallbackTaxFormMetricRule
{
    public List<string> Check(FallbackTaxFormMetric item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback tax form metric.</summary>
public sealed record FallbackTaxFormMetricValidator(List<IFallbackTaxFormMetricRule> Rules)
{
    public List<string> Validate(FallbackTaxFormMetric item)
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

    public void RequireValid(FallbackTaxFormMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTaxFormMetric> items)
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

/// <summary>Functions of fallback tax form metric rules.</summary>
public static class FallbackTaxFormMetricRules
{
    public static FallbackTaxFormMetricValidator DefaultFallbackTaxFormMetricValidator()
    {
        return new FallbackTaxFormMetricValidator(
            new List<IFallbackTaxFormMetricRule>
            {
                new FallbackTaxFormMetricLocaleLengthRule(86400),
                new FallbackTaxFormMetricRequiredCodeRule("fallback tax form metric needs this field"),
                new FallbackTaxFormMetricRequiredLocaleRule("fallback tax form metric needs this field"),
                new FallbackTaxFormMetricNotesLengthRule(64),
                new FallbackTaxFormMetricCostRangeRule(4.6, 2163.1),
            });
    }
}
