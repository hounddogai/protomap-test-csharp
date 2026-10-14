namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback occupancy adjustment must pass. Check returns its problems, or none.</summary>
public interface IFallbackOccupancyAdjustmentRule
{
    List<string> Check(FallbackOccupancyAdjustment item);
}

public sealed record FallbackOccupancyAdjustmentCodeLengthRule(int MaxLength) : IFallbackOccupancyAdjustmentRule
{
    public List<string> Check(FallbackOccupancyAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackOccupancyAdjustmentLabelLengthRule(int MaxLength) : IFallbackOccupancyAdjustmentRule
{
    public List<string> Check(FallbackOccupancyAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackOccupancyAdjustmentStartsAtRangeRule(int Min, int Max) : IFallbackOccupancyAdjustmentRule
{
    public List<string> Check(FallbackOccupancyAdjustment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackOccupancyAdjustmentRequiredCodeRule(string Message) : IFallbackOccupancyAdjustmentRule
{
    public List<string> Check(FallbackOccupancyAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackOccupancyAdjustmentAllowedCodeRule(List<string> Allowed) : IFallbackOccupancyAdjustmentRule
{
    public List<string> Check(FallbackOccupancyAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback occupancy adjustment.</summary>
public sealed record FallbackOccupancyAdjustmentValidator(List<IFallbackOccupancyAdjustmentRule> Rules)
{
    public List<string> Validate(FallbackOccupancyAdjustment item)
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

    public void RequireValid(FallbackOccupancyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackOccupancyAdjustment> items)
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

/// <summary>Functions of fallback occupancy adjustment rules.</summary>
public static class FallbackOccupancyAdjustmentRules
{
    public static FallbackOccupancyAdjustmentValidator DefaultFallbackOccupancyAdjustmentValidator()
    {
        return new FallbackOccupancyAdjustmentValidator(
            new List<IFallbackOccupancyAdjustmentRule>
            {
                new FallbackOccupancyAdjustmentCodeLengthRule(3600),
                new FallbackOccupancyAdjustmentLabelLengthRule(120),
                new FallbackOccupancyAdjustmentStartsAtRangeRule(255, 1000),
                new FallbackOccupancyAdjustmentRequiredCodeRule("fallback occupancy adjustment needs a value"),
                new FallbackOccupancyAdjustmentAllowedCodeRule(new List<string>()),
            });
    }
}
