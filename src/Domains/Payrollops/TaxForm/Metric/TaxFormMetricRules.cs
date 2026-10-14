namespace Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>A rule a tax form metric must pass. Check returns its problems, or none.</summary>
public interface ITaxFormMetricRule
{
    List<string> Check(TaxFormMetric item);
}

public sealed record TaxFormMetricRequiredLabelRule(string Message) : ITaxFormMetricRule
{
    public List<string> Check(TaxFormMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMetricRequiredCodeRule(string Message) : ITaxFormMetricRule
{
    public List<string> Check(TaxFormMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMetricDurationMinutesRangeRule(int Min, int Max) : ITaxFormMetricRule
{
    public List<string> Check(TaxFormMetric item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormMetricRequiredLocaleRule(string Message) : ITaxFormMetricRule
{
    public List<string> Check(TaxFormMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMetricRequiredCurrencyRule(string Message) : ITaxFormMetricRule
{
    public List<string> Check(TaxFormMetric item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form metric.</summary>
public sealed record TaxFormMetricValidator(List<ITaxFormMetricRule> Rules)
{
    public List<string> Validate(TaxFormMetric item)
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

    public void RequireValid(TaxFormMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormMetric> items)
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

/// <summary>Functions of tax form metric rules.</summary>
public static class TaxFormMetricRules
{
    public static TaxFormMetricValidator DefaultTaxFormMetricValidator()
    {
        return new TaxFormMetricValidator(
            new List<ITaxFormMetricRule>
            {
                new TaxFormMetricRequiredLabelRule("tax form metric needs an entry"),
                new TaxFormMetricRequiredCodeRule("tax form metric needs a value"),
                new TaxFormMetricDurationMinutesRangeRule(16, 64),
                new TaxFormMetricRequiredLocaleRule("tax form metric needs this field"),
                new TaxFormMetricRequiredCurrencyRule("tax form metric needs this field"),
            });
    }
}
