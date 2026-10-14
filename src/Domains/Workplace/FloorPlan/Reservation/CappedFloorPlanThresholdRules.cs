namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped floor plan threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedFloorPlanThresholdRule
{
    List<string> Check(CappedFloorPlanThreshold item);
}

public sealed record CappedFloorPlanThresholdCodePrefixRule(string Prefix) : ICappedFloorPlanThresholdRule
{
    public List<string> Check(CappedFloorPlanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFloorPlanThresholdRequiredPhoneRule(string Message) : ICappedFloorPlanThresholdRule
{
    public List<string> Check(CappedFloorPlanThreshold item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedFloorPlanThresholdAllowedLabelRule(List<string> Allowed) : ICappedFloorPlanThresholdRule
{
    public List<string> Check(CappedFloorPlanThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record CappedFloorPlanThresholdAttemptsRangeRule(int Min, int Max) : ICappedFloorPlanThresholdRule
{
    public List<string> Check(CappedFloorPlanThreshold item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedFloorPlanThresholdRequiredCodeRule(string Message) : ICappedFloorPlanThresholdRule
{
    public List<string> Check(CappedFloorPlanThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped floor plan threshold.</summary>
public sealed record CappedFloorPlanThresholdValidator(List<ICappedFloorPlanThresholdRule> Rules)
{
    public List<string> Validate(CappedFloorPlanThreshold item)
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

    public void RequireValid(CappedFloorPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedFloorPlanThreshold> items)
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

/// <summary>Functions of capped floor plan threshold rules.</summary>
public static class CappedFloorPlanThresholdRules
{
    public static CappedFloorPlanThresholdValidator DefaultCappedFloorPlanThresholdValidator()
    {
        return new CappedFloorPlanThresholdValidator(
            new List<ICappedFloorPlanThresholdRule>
            {
                new CappedFloorPlanThresholdCodePrefixRule("REF"),
                new CappedFloorPlanThresholdRequiredPhoneRule("capped floor plan threshold needs an entry"),
                new CappedFloorPlanThresholdAllowedLabelRule(new List<string> { "weighted", "tiered", "global" }),
                new CappedFloorPlanThresholdAttemptsRangeRule(1000, 3600),
                new CappedFloorPlanThresholdRequiredCodeRule("capped floor plan threshold needs an entry"),
            });
    }
}
