namespace Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>A rule a accrual metric must pass. Check returns its problems, or none.</summary>
public interface IAccrualMetricRule
{
    List<string> Check(AccrualMetric item);
}

public sealed record AccrualMetricGraceDaysRangeRule(int Min, int Max) : IAccrualMetricRule
{
    public List<string> Check(AccrualMetric item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualMetricActiveGuardRule(double Limit) : IAccrualMetricRule
{
    public List<string> Check(AccrualMetric item)
    {
        if (item.Active && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AccrualMetricRequiredLabelRule(string Message) : IAccrualMetricRule
{
    public List<string> Check(AccrualMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualMetricCodePrefixRule(string Prefix) : IAccrualMetricRule
{
    public List<string> Check(AccrualMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual metric.</summary>
public sealed record AccrualMetricValidator(List<IAccrualMetricRule> Rules)
{
    public List<string> Validate(AccrualMetric item)
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

    public void RequireValid(AccrualMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualMetric> items)
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

/// <summary>Functions of accrual metric rules.</summary>
public static class AccrualMetricRules
{
    public static AccrualMetricValidator DefaultAccrualMetricValidator()
    {
        return new AccrualMetricValidator(
            new List<IAccrualMetricRule>
            {
                new AccrualMetricGraceDaysRangeRule(16, 64),
                new AccrualMetricActiveGuardRule(9.0),
                new AccrualMetricRequiredLabelRule("accrual metric needs a value"),
                new AccrualMetricCodePrefixRule("OPS"),
            });
    }
}
