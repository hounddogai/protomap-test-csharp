namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback leave request adjustment must pass. Check returns its problems, or none.</summary>
public interface IFallbackLeaveRequestAdjustmentRule
{
    List<string> Check(FallbackLeaveRequestAdjustment item);
}

public sealed record FallbackLeaveRequestAdjustmentRatioRangeRule(double Min, double Max) : IFallbackLeaveRequestAdjustmentRule
{
    public List<string> Check(FallbackLeaveRequestAdjustment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestAdjustmentRequiredCodeRule(string Message) : IFallbackLeaveRequestAdjustmentRule
{
    public List<string> Check(FallbackLeaveRequestAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestAdjustmentAllowedCountryRule(List<string> Allowed) : IFallbackLeaveRequestAdjustmentRule
{
    public List<string> Check(FallbackLeaveRequestAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestAdjustmentLabelLengthRule(int MaxLength) : IFallbackLeaveRequestAdjustmentRule
{
    public List<string> Check(FallbackLeaveRequestAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestAdjustmentRequiredCountryRule(string Message) : IFallbackLeaveRequestAdjustmentRule
{
    public List<string> Check(FallbackLeaveRequestAdjustment item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback leave request adjustment.</summary>
public sealed record FallbackLeaveRequestAdjustmentValidator(List<IFallbackLeaveRequestAdjustmentRule> Rules)
{
    public List<string> Validate(FallbackLeaveRequestAdjustment item)
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

    public void RequireValid(FallbackLeaveRequestAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLeaveRequestAdjustment> items)
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

/// <summary>Functions of fallback leave request adjustment rules.</summary>
public static class FallbackLeaveRequestAdjustmentRules
{
    public static FallbackLeaveRequestAdjustmentValidator DefaultFallbackLeaveRequestAdjustmentValidator()
    {
        return new FallbackLeaveRequestAdjustmentValidator(
            new List<IFallbackLeaveRequestAdjustmentRule>
            {
                new FallbackLeaveRequestAdjustmentRatioRangeRule(9.7, 3016.2),
                new FallbackLeaveRequestAdjustmentRequiredCodeRule(
                    "fallback leave request adjustment needs an entry"),
                new FallbackLeaveRequestAdjustmentAllowedCountryRule(
                    new List<string> { "default", "capped", "fallback" }),
                new FallbackLeaveRequestAdjustmentLabelLengthRule(120),
                new FallbackLeaveRequestAdjustmentRequiredCountryRule(
                    "fallback leave request adjustment needs an entry"),
            });
    }
}
