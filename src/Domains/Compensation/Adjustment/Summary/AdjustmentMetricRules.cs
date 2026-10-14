namespace Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>A rule a adjustment metric must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentMetricRule
{
    List<string> Check(AdjustmentMetric item);
}

public sealed record AdjustmentMetricCodeLengthRule(int MaxLength) : IAdjustmentMetricRule
{
    public List<string> Check(AdjustmentMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMetricScoreRangeRule(double Min, double Max) : IAdjustmentMetricRule
{
    public List<string> Check(AdjustmentMetric item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentMetricCurrencyLengthRule(int MaxLength) : IAdjustmentMetricRule
{
    public List<string> Check(AdjustmentMetric item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMetricTitleLengthRule(int MaxLength) : IAdjustmentMetricRule
{
    public List<string> Check(AdjustmentMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMetricRequiredTitleRule(string Message) : IAdjustmentMetricRule
{
    public List<string> Check(AdjustmentMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment metric.</summary>
public sealed record AdjustmentMetricValidator(List<IAdjustmentMetricRule> Rules)
{
    public List<string> Validate(AdjustmentMetric item)
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

    public void RequireValid(AdjustmentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentMetric> items)
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

/// <summary>Functions of adjustment metric rules.</summary>
public static class AdjustmentMetricRules
{
    public static AdjustmentMetricValidator DefaultAdjustmentMetricValidator()
    {
        return new AdjustmentMetricValidator(
            new List<IAdjustmentMetricRule>
            {
                new AdjustmentMetricCodeLengthRule(3600),
                new AdjustmentMetricScoreRangeRule(8.4, 337.1),
                new AdjustmentMetricCurrencyLengthRule(3600),
                new AdjustmentMetricTitleLengthRule(120),
                new AdjustmentMetricRequiredTitleRule("adjustment metric needs a value"),
            });
    }
}
