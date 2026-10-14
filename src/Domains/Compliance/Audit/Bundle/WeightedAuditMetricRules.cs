namespace Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>A rule a weighted audit metric must pass. Check returns its problems, or none.</summary>
public interface IWeightedAuditMetricRule
{
    List<string> Check(WeightedAuditMetric item);
}

public sealed record WeightedAuditMetricSegmentLengthRule(int MaxLength) : IWeightedAuditMetricRule
{
    public List<string> Check(WeightedAuditMetric item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditMetricBudgetRangeRule(double Min, double Max) : IWeightedAuditMetricRule
{
    public List<string> Check(WeightedAuditMetric item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditMetricRequiredCodeRule(string Message) : IWeightedAuditMetricRule
{
    public List<string> Check(WeightedAuditMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditMetricApprovedGuardRule(int Limit) : IWeightedAuditMetricRule
{
    public List<string> Check(WeightedAuditMetric item)
    {
        if (item.Approved && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted audit metric.</summary>
public sealed record WeightedAuditMetricValidator(List<IWeightedAuditMetricRule> Rules)
{
    public List<string> Validate(WeightedAuditMetric item)
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

    public void RequireValid(WeightedAuditMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAuditMetric> items)
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

/// <summary>Functions of weighted audit metric rules.</summary>
public static class WeightedAuditMetricRules
{
    public static WeightedAuditMetricValidator DefaultWeightedAuditMetricValidator()
    {
        return new WeightedAuditMetricValidator(
            new List<IWeightedAuditMetricRule>
            {
                new WeightedAuditMetricSegmentLengthRule(16),
                new WeightedAuditMetricBudgetRangeRule(4.4, 3974.8),
                new WeightedAuditMetricRequiredCodeRule("weighted audit metric needs this field"),
                new WeightedAuditMetricApprovedGuardRule(255),
            });
    }
}
