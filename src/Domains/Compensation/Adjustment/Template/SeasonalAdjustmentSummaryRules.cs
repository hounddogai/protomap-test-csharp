namespace Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>A rule a seasonal adjustment summary must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAdjustmentSummaryRule
{
    List<string> Check(SeasonalAdjustmentSummary item);
}

public sealed record SeasonalAdjustmentSummaryApprovedGuardRule(double Limit) : ISeasonalAdjustmentSummaryRule
{
    public List<string> Check(SeasonalAdjustmentSummary item)
    {
        if (item.Approved && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSummaryTitleLengthRule(int MaxLength) : ISeasonalAdjustmentSummaryRule
{
    public List<string> Check(SeasonalAdjustmentSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSummaryRateRangeRule(double Min, double Max) : ISeasonalAdjustmentSummaryRule
{
    public List<string> Check(SeasonalAdjustmentSummary item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSummaryMaximumRangeRule(double Min, double Max) : ISeasonalAdjustmentSummaryRule
{
    public List<string> Check(SeasonalAdjustmentSummary item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentSummaryVersionRangeRule(int Min, int Max) : ISeasonalAdjustmentSummaryRule
{
    public List<string> Check(SeasonalAdjustmentSummary item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal adjustment summary.</summary>
public sealed record SeasonalAdjustmentSummaryValidator(List<ISeasonalAdjustmentSummaryRule> Rules)
{
    public List<string> Validate(SeasonalAdjustmentSummary item)
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

    public void RequireValid(SeasonalAdjustmentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAdjustmentSummary> items)
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

/// <summary>Functions of seasonal adjustment summary rules.</summary>
public static class SeasonalAdjustmentSummaryRules
{
    public static SeasonalAdjustmentSummaryValidator DefaultSeasonalAdjustmentSummaryValidator()
    {
        return new SeasonalAdjustmentSummaryValidator(
            new List<ISeasonalAdjustmentSummaryRule>
            {
                new SeasonalAdjustmentSummaryApprovedGuardRule(2.8),
                new SeasonalAdjustmentSummaryTitleLengthRule(86400),
                new SeasonalAdjustmentSummaryRateRangeRule(5.1, 4260.5),
                new SeasonalAdjustmentSummaryMaximumRangeRule(9.4, 1825.2),
                new SeasonalAdjustmentSummaryVersionRangeRule(16, 32),
            });
    }
}
