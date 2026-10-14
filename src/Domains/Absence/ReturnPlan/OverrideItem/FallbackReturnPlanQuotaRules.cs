namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>A rule a fallback return plan quota must pass. Check returns its problems, or none.</summary>
public interface IFallbackReturnPlanQuotaRule
{
    List<string> Check(FallbackReturnPlanQuota item);
}

public sealed record FallbackReturnPlanQuotaQuantityRangeRule(int Min, int Max) : IFallbackReturnPlanQuotaRule
{
    public List<string> Check(FallbackReturnPlanQuota item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackReturnPlanQuotaSegmentLengthRule(int MaxLength) : IFallbackReturnPlanQuotaRule
{
    public List<string> Check(FallbackReturnPlanQuota item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackReturnPlanQuotaLocationLengthRule(int MaxLength) : IFallbackReturnPlanQuotaRule
{
    public List<string> Check(FallbackReturnPlanQuota item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback return plan quota.</summary>
public sealed record FallbackReturnPlanQuotaValidator(List<IFallbackReturnPlanQuotaRule> Rules)
{
    public List<string> Validate(FallbackReturnPlanQuota item)
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

    public void RequireValid(FallbackReturnPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackReturnPlanQuota> items)
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

/// <summary>Functions of fallback return plan quota rules.</summary>
public static class FallbackReturnPlanQuotaRules
{
    public static FallbackReturnPlanQuotaValidator DefaultFallbackReturnPlanQuotaValidator()
    {
        return new FallbackReturnPlanQuotaValidator(
            new List<IFallbackReturnPlanQuotaRule>
            {
                new FallbackReturnPlanQuotaQuantityRangeRule(16, 80),
                new FallbackReturnPlanQuotaSegmentLengthRule(80),
                new FallbackReturnPlanQuotaLocationLengthRule(3600),
            });
    }
}
