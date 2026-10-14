namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>A rule a adjustment milestone must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentMilestoneRule
{
    List<string> Check(AdjustmentMilestone item);
}

public sealed record AdjustmentMilestoneRequiredLabelRule(string Message) : IAdjustmentMilestoneRule
{
    public List<string> Check(AdjustmentMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMilestoneRatioRangeRule(double Min, double Max) : IAdjustmentMilestoneRule
{
    public List<string> Check(AdjustmentMilestone item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentMilestoneRequiredCodeRule(string Message) : IAdjustmentMilestoneRule
{
    public List<string> Check(AdjustmentMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMilestoneLabelLengthRule(int MaxLength) : IAdjustmentMilestoneRule
{
    public List<string> Check(AdjustmentMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMilestoneFteRangeRule(double Min, double Max) : IAdjustmentMilestoneRule
{
    public List<string> Check(AdjustmentMilestone item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment milestone.</summary>
public sealed record AdjustmentMilestoneValidator(List<IAdjustmentMilestoneRule> Rules)
{
    public List<string> Validate(AdjustmentMilestone item)
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

    public void RequireValid(AdjustmentMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentMilestone> items)
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

/// <summary>Functions of adjustment milestone rules.</summary>
public static class AdjustmentMilestoneRules
{
    public static AdjustmentMilestoneValidator DefaultAdjustmentMilestoneValidator()
    {
        return new AdjustmentMilestoneValidator(
            new List<IAdjustmentMilestoneRule>
            {
                new AdjustmentMilestoneRequiredLabelRule("adjustment milestone needs an entry"),
                new AdjustmentMilestoneRatioRangeRule(5.3, 680.3),
                new AdjustmentMilestoneRequiredCodeRule("adjustment milestone needs this field"),
                new AdjustmentMilestoneLabelLengthRule(80),
                new AdjustmentMilestoneFteRangeRule(0.2, 496.1),
            });
    }
}
