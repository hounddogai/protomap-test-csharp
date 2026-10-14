namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>A rule a adaptive return plan threshold must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveReturnPlanThresholdRule
{
    List<string> Check(AdaptiveReturnPlanThreshold item);
}

public sealed record AdaptiveReturnPlanThresholdPriorityRangeRule(int Min, int Max) : IAdaptiveReturnPlanThresholdRule
{
    public List<string> Check(AdaptiveReturnPlanThreshold item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReturnPlanThresholdExpiresAtRangeRule(int Min, int Max) : IAdaptiveReturnPlanThresholdRule
{
    public List<string> Check(AdaptiveReturnPlanThreshold item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReturnPlanThresholdCodePrefixRule(string Prefix) : IAdaptiveReturnPlanThresholdRule
{
    public List<string> Check(AdaptiveReturnPlanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveReturnPlanThresholdDepartmentLengthRule(int MaxLength) : IAdaptiveReturnPlanThresholdRule
{
    public List<string> Check(AdaptiveReturnPlanThreshold item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive return plan threshold.</summary>
public sealed record AdaptiveReturnPlanThresholdValidator(List<IAdaptiveReturnPlanThresholdRule> Rules)
{
    public List<string> Validate(AdaptiveReturnPlanThreshold item)
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

    public void RequireValid(AdaptiveReturnPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveReturnPlanThreshold> items)
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

/// <summary>Functions of adaptive return plan threshold rules.</summary>
public static class AdaptiveReturnPlanThresholdRules
{
    public static AdaptiveReturnPlanThresholdValidator DefaultAdaptiveReturnPlanThresholdValidator()
    {
        return new AdaptiveReturnPlanThresholdValidator(
            new List<IAdaptiveReturnPlanThresholdRule>
            {
                new AdaptiveReturnPlanThresholdPriorityRangeRule(64, 255),
                new AdaptiveReturnPlanThresholdExpiresAtRangeRule(32, 80),
                new AdaptiveReturnPlanThresholdCodePrefixRule("REF"),
                new AdaptiveReturnPlanThresholdDepartmentLengthRule(120),
            });
    }
}
