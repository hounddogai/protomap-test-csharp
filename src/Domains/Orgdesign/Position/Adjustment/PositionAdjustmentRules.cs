namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position adjustment must pass. Check returns its problems, or none.</summary>
public interface IPositionAdjustmentRule
{
    List<string> Check(PositionAdjustment item);
}

public sealed record PositionAdjustmentAllowedJobCodeRule(List<string> Allowed) : IPositionAdjustmentRule
{
    public List<string> Check(PositionAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PositionAdjustmentHoursRangeRule(double Min, double Max) : IPositionAdjustmentRule
{
    public List<string> Check(PositionAdjustment item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionAdjustmentCodeLengthRule(int MaxLength) : IPositionAdjustmentRule
{
    public List<string> Check(PositionAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionAdjustmentRequiredCodeRule(string Message) : IPositionAdjustmentRule
{
    public List<string> Check(PositionAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionAdjustmentNameLengthRule(int MaxLength) : IPositionAdjustmentRule
{
    public List<string> Check(PositionAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position adjustment.</summary>
public sealed record PositionAdjustmentValidator(List<IPositionAdjustmentRule> Rules)
{
    public List<string> Validate(PositionAdjustment item)
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

    public void RequireValid(PositionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionAdjustment> items)
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

/// <summary>Functions of position adjustment rules.</summary>
public static class PositionAdjustmentRules
{
    public static PositionAdjustmentValidator DefaultPositionAdjustmentValidator()
    {
        return new PositionAdjustmentValidator(
            new List<IPositionAdjustmentRule>
            {
                new PositionAdjustmentAllowedJobCodeRule(new List<string>()),
                new PositionAdjustmentHoursRangeRule(4.0, 2276.5),
                new PositionAdjustmentCodeLengthRule(86400),
                new PositionAdjustmentRequiredCodeRule("position adjustment needs this field"),
                new PositionAdjustmentNameLengthRule(86400),
            });
    }
}
