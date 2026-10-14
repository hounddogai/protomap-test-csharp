namespace Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>A rule a adaptive return plan metric must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveReturnPlanMetricRule
{
    List<string> Check(AdaptiveReturnPlanMetric item);
}

public sealed record AdaptiveReturnPlanMetricTeamLengthRule(int MaxLength) : IAdaptiveReturnPlanMetricRule
{
    public List<string> Check(AdaptiveReturnPlanMetric item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReturnPlanMetricScoreRangeRule(double Min, double Max) : IAdaptiveReturnPlanMetricRule
{
    public List<string> Check(AdaptiveReturnPlanMetric item)
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

public sealed record AdaptiveReturnPlanMetricRequiredChecksumRule(string Message) : IAdaptiveReturnPlanMetricRule
{
    public List<string> Check(AdaptiveReturnPlanMetric item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReturnPlanMetricCodeLengthRule(int MaxLength) : IAdaptiveReturnPlanMetricRule
{
    public List<string> Check(AdaptiveReturnPlanMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive return plan metric.</summary>
public sealed record AdaptiveReturnPlanMetricValidator(List<IAdaptiveReturnPlanMetricRule> Rules)
{
    public List<string> Validate(AdaptiveReturnPlanMetric item)
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

    public void RequireValid(AdaptiveReturnPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveReturnPlanMetric> items)
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

/// <summary>Functions of adaptive return plan metric rules.</summary>
public static class AdaptiveReturnPlanMetricRules
{
    public static AdaptiveReturnPlanMetricValidator DefaultAdaptiveReturnPlanMetricValidator()
    {
        return new AdaptiveReturnPlanMetricValidator(
            new List<IAdaptiveReturnPlanMetricRule>
            {
                new AdaptiveReturnPlanMetricTeamLengthRule(3600),
                new AdaptiveReturnPlanMetricScoreRangeRule(6.3, 2461.2),
                new AdaptiveReturnPlanMetricRequiredChecksumRule("adaptive return plan metric needs an entry"),
                new AdaptiveReturnPlanMetricCodeLengthRule(120),
            });
    }
}
