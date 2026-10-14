namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>A rule a manual tax form metric must pass. Check returns its problems, or none.</summary>
public interface IManualTaxFormMetricRule
{
    List<string> Check(ManualTaxFormMetric item);
}

public sealed record ManualTaxFormMetricCodeLengthRule(int MaxLength) : IManualTaxFormMetricRule
{
    public List<string> Check(ManualTaxFormMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormMetricRatingRangeRule(double Min, double Max) : IManualTaxFormMetricRule
{
    public List<string> Check(ManualTaxFormMetric item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormMetricTitleLengthRule(int MaxLength) : IManualTaxFormMetricRule
{
    public List<string> Check(ManualTaxFormMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual tax form metric.</summary>
public sealed record ManualTaxFormMetricValidator(List<IManualTaxFormMetricRule> Rules)
{
    public List<string> Validate(ManualTaxFormMetric item)
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

    public void RequireValid(ManualTaxFormMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualTaxFormMetric> items)
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

/// <summary>Functions of manual tax form metric rules.</summary>
public static class ManualTaxFormMetricRules
{
    public static ManualTaxFormMetricValidator DefaultManualTaxFormMetricValidator()
    {
        return new ManualTaxFormMetricValidator(
            new List<IManualTaxFormMetricRule>
            {
                new ManualTaxFormMetricCodeLengthRule(3600),
                new ManualTaxFormMetricRatingRangeRule(4.8, 1166.0),
                new ManualTaxFormMetricTitleLengthRule(86400),
            });
    }
}
