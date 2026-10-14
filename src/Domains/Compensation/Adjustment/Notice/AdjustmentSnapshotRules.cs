namespace Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>A rule a adjustment snapshot must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentSnapshotRule
{
    List<string> Check(AdjustmentSnapshot item);
}

public sealed record AdjustmentSnapshotRequiredLabelRule(string Message) : IAdjustmentSnapshotRule
{
    public List<string> Check(AdjustmentSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSnapshotRequiredCodeRule(string Message) : IAdjustmentSnapshotRule
{
    public List<string> Check(AdjustmentSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSnapshotFloorValueRangeRule(double Min, double Max) : IAdjustmentSnapshotRule
{
    public List<string> Check(AdjustmentSnapshot item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSnapshotCodeLengthRule(int MaxLength) : IAdjustmentSnapshotRule
{
    public List<string> Check(AdjustmentSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSnapshotDiscountRangeRule(double Min, double Max) : IAdjustmentSnapshotRule
{
    public List<string> Check(AdjustmentSnapshot item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment snapshot.</summary>
public sealed record AdjustmentSnapshotValidator(List<IAdjustmentSnapshotRule> Rules)
{
    public List<string> Validate(AdjustmentSnapshot item)
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

    public void RequireValid(AdjustmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentSnapshot> items)
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

/// <summary>Functions of adjustment snapshot rules.</summary>
public static class AdjustmentSnapshotRules
{
    public static AdjustmentSnapshotValidator DefaultAdjustmentSnapshotValidator()
    {
        return new AdjustmentSnapshotValidator(
            new List<IAdjustmentSnapshotRule>
            {
                new AdjustmentSnapshotRequiredLabelRule("adjustment snapshot needs this field"),
                new AdjustmentSnapshotRequiredCodeRule("adjustment snapshot needs an entry"),
                new AdjustmentSnapshotFloorValueRangeRule(1.7, 1727.3),
                new AdjustmentSnapshotCodeLengthRule(32),
                new AdjustmentSnapshotDiscountRangeRule(9.0, 264.2),
            });
    }
}
