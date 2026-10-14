namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>A rule a global position adjustment must pass. Check returns its problems, or none.</summary>
public interface IGlobalPositionAdjustmentRule
{
    List<string> Check(GlobalPositionAdjustment item);
}

public sealed record GlobalPositionAdjustmentNameLengthRule(int MaxLength) : IGlobalPositionAdjustmentRule
{
    public List<string> Check(GlobalPositionAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionAdjustmentPriorityRangeRule(int Min, int Max) : IGlobalPositionAdjustmentRule
{
    public List<string> Check(GlobalPositionAdjustment item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalPositionAdjustmentSegmentLengthRule(int MaxLength) : IGlobalPositionAdjustmentRule
{
    public List<string> Check(GlobalPositionAdjustment item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global position adjustment.</summary>
public sealed record GlobalPositionAdjustmentValidator(List<IGlobalPositionAdjustmentRule> Rules)
{
    public List<string> Validate(GlobalPositionAdjustment item)
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

    public void RequireValid(GlobalPositionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPositionAdjustment> items)
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

/// <summary>Functions of global position adjustment rules.</summary>
public static class GlobalPositionAdjustmentRules
{
    public static GlobalPositionAdjustmentValidator DefaultGlobalPositionAdjustmentValidator()
    {
        return new GlobalPositionAdjustmentValidator(
            new List<IGlobalPositionAdjustmentRule>
            {
                new GlobalPositionAdjustmentNameLengthRule(80),
                new GlobalPositionAdjustmentPriorityRangeRule(3600, 86400),
                new GlobalPositionAdjustmentSegmentLengthRule(255),
            });
    }
}
