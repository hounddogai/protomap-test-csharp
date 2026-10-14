namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>A rule a adjustment rollup must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentRollupRule
{
    List<string> Check(AdjustmentRollup item);
}

public sealed record AdjustmentRollupRequiredLabelRule(string Message) : IAdjustmentRollupRule
{
    public List<string> Check(AdjustmentRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRollupLeaveDaysRangeRule(int Min, int Max) : IAdjustmentRollupRule
{
    public List<string> Check(AdjustmentRollup item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentRollupSegmentLengthRule(int MaxLength) : IAdjustmentRollupRule
{
    public List<string> Check(AdjustmentRollup item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment rollup.</summary>
public sealed record AdjustmentRollupValidator(List<IAdjustmentRollupRule> Rules)
{
    public List<string> Validate(AdjustmentRollup item)
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

    public void RequireValid(AdjustmentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentRollup> items)
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

/// <summary>Functions of adjustment rollup rules.</summary>
public static class AdjustmentRollupRules
{
    public static AdjustmentRollupValidator DefaultAdjustmentRollupValidator()
    {
        return new AdjustmentRollupValidator(
            new List<IAdjustmentRollupRule>
            {
                new AdjustmentRollupRequiredLabelRule("adjustment rollup needs a value"),
                new AdjustmentRollupLeaveDaysRangeRule(120, 255),
                new AdjustmentRollupSegmentLengthRule(120),
            });
    }
}
