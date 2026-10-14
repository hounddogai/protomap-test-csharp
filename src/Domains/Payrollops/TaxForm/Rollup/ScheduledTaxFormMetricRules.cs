namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>A rule a scheduled tax form metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledTaxFormMetricRule
{
    List<string> Check(ScheduledTaxFormMetric item);
}

public sealed record ScheduledTaxFormMetricSourceLengthRule(int MaxLength) : IScheduledTaxFormMetricRule
{
    public List<string> Check(ScheduledTaxFormMetric item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormMetricCodeLengthRule(int MaxLength) : IScheduledTaxFormMetricRule
{
    public List<string> Check(ScheduledTaxFormMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormMetricNotesLengthRule(int MaxLength) : IScheduledTaxFormMetricRule
{
    public List<string> Check(ScheduledTaxFormMetric item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormMetricAmountRangeRule(double Min, double Max) : IScheduledTaxFormMetricRule
{
    public List<string> Check(ScheduledTaxFormMetric item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled tax form metric.</summary>
public sealed record ScheduledTaxFormMetricValidator(List<IScheduledTaxFormMetricRule> Rules)
{
    public List<string> Validate(ScheduledTaxFormMetric item)
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

    public void RequireValid(ScheduledTaxFormMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTaxFormMetric> items)
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

/// <summary>Functions of scheduled tax form metric rules.</summary>
public static class ScheduledTaxFormMetricRules
{
    public static ScheduledTaxFormMetricValidator DefaultScheduledTaxFormMetricValidator()
    {
        return new ScheduledTaxFormMetricValidator(
            new List<IScheduledTaxFormMetricRule>
            {
                new ScheduledTaxFormMetricSourceLengthRule(64),
                new ScheduledTaxFormMetricCodeLengthRule(86400),
                new ScheduledTaxFormMetricNotesLengthRule(80),
                new ScheduledTaxFormMetricAmountRangeRule(2.0, 640.1),
            });
    }
}
