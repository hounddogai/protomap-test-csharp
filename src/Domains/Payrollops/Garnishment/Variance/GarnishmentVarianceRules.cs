namespace Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>A rule a garnishment variance must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentVarianceRule
{
    List<string> Check(GarnishmentVariance item);
}

public sealed record GarnishmentVarianceRequiredLabelRule(string Message) : IGarnishmentVarianceRule
{
    public List<string> Check(GarnishmentVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentVarianceTenureMonthsRangeRule(int Min, int Max) : IGarnishmentVarianceRule
{
    public List<string> Check(GarnishmentVariance item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentVarianceUpdatedAtRangeRule(int Min, int Max) : IGarnishmentVarianceRule
{
    public List<string> Check(GarnishmentVariance item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment variance.</summary>
public sealed record GarnishmentVarianceValidator(List<IGarnishmentVarianceRule> Rules)
{
    public List<string> Validate(GarnishmentVariance item)
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

    public void RequireValid(GarnishmentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentVariance> items)
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

/// <summary>Functions of garnishment variance rules.</summary>
public static class GarnishmentVarianceRules
{
    public static GarnishmentVarianceValidator DefaultGarnishmentVarianceValidator()
    {
        return new GarnishmentVarianceValidator(
            new List<IGarnishmentVarianceRule>
            {
                new GarnishmentVarianceRequiredLabelRule("garnishment variance needs this field"),
                new GarnishmentVarianceTenureMonthsRangeRule(1000, 3600),
                new GarnishmentVarianceUpdatedAtRangeRule(16, 86400),
            });
    }
}
