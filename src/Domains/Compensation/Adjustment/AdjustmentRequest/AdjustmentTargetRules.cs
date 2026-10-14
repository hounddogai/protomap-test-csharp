namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>A rule a adjustment target must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentTargetRule
{
    List<string> Check(AdjustmentTarget item);
}

public sealed record AdjustmentTargetRequiredRegionRule(string Message) : IAdjustmentTargetRule
{
    public List<string> Check(AdjustmentTarget item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTargetCapacityRangeRule(int Min, int Max) : IAdjustmentTargetRule
{
    public List<string> Check(AdjustmentTarget item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTargetDepartmentLengthRule(int MaxLength) : IAdjustmentTargetRule
{
    public List<string> Check(AdjustmentTarget item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTargetArchivedGuardRule(int Limit) : IAdjustmentTargetRule
{
    public List<string> Check(AdjustmentTarget item)
    {
        if (item.Archived && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTargetLabelLengthRule(int MaxLength) : IAdjustmentTargetRule
{
    public List<string> Check(AdjustmentTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment target.</summary>
public sealed record AdjustmentTargetValidator(List<IAdjustmentTargetRule> Rules)
{
    public List<string> Validate(AdjustmentTarget item)
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

    public void RequireValid(AdjustmentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentTarget> items)
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

/// <summary>Functions of adjustment target rules.</summary>
public static class AdjustmentTargetRules
{
    public static AdjustmentTargetValidator DefaultAdjustmentTargetValidator()
    {
        return new AdjustmentTargetValidator(
            new List<IAdjustmentTargetRule>
            {
                new AdjustmentTargetRequiredRegionRule("adjustment target needs this field"),
                new AdjustmentTargetCapacityRangeRule(255, 255),
                new AdjustmentTargetDepartmentLengthRule(120),
                new AdjustmentTargetArchivedGuardRule(1000),
                new AdjustmentTargetLabelLengthRule(16),
            });
    }
}
