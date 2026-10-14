namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling floor plan bundle must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanBundleRule
{
    List<string> Check(RollingFloorPlanBundle item);
}

public sealed record RollingFloorPlanBundleRequiredBadgeRule(string Message) : IRollingFloorPlanBundleRule
{
    public List<string> Check(RollingFloorPlanBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBundleFteRangeRule(double Min, double Max) : IRollingFloorPlanBundleRule
{
    public List<string> Check(RollingFloorPlanBundle item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFloorPlanBundleAllowedCurrencyRule(List<string> Allowed) : IRollingFloorPlanBundleRule
{
    public List<string> Check(RollingFloorPlanBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBundleBadgeLengthRule(int MaxLength) : IRollingFloorPlanBundleRule
{
    public List<string> Check(RollingFloorPlanBundle item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanBundleCodePrefixRule(string Prefix) : IRollingFloorPlanBundleRule
{
    public List<string> Check(RollingFloorPlanBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling floor plan bundle.</summary>
public sealed record RollingFloorPlanBundleValidator(List<IRollingFloorPlanBundleRule> Rules)
{
    public List<string> Validate(RollingFloorPlanBundle item)
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

    public void RequireValid(RollingFloorPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanBundle> items)
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

/// <summary>Functions of rolling floor plan bundle rules.</summary>
public static class RollingFloorPlanBundleRules
{
    public static RollingFloorPlanBundleValidator DefaultRollingFloorPlanBundleValidator()
    {
        return new RollingFloorPlanBundleValidator(
            new List<IRollingFloorPlanBundleRule>
            {
                new RollingFloorPlanBundleRequiredBadgeRule("rolling floor plan bundle needs a value"),
                new RollingFloorPlanBundleFteRangeRule(4.6, 3141.7),
                new RollingFloorPlanBundleAllowedCurrencyRule(
                    new List<string> { "regional", "scheduled", "primary" }),
                new RollingFloorPlanBundleBadgeLengthRule(255),
                new RollingFloorPlanBundleCodePrefixRule("X"),
            });
    }
}
