namespace Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>A rule a engagement slot must pass. Check returns its problems, or none.</summary>
public interface IEngagementSlotRule
{
    List<string> Check(EngagementSlot item);
}

public sealed record EngagementSlotWeightRangeRule(double Min, double Max) : IEngagementSlotRule
{
    public List<string> Check(EngagementSlot item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementSlotVisibleGuardRule(int Limit) : IEngagementSlotRule
{
    public List<string> Check(EngagementSlot item)
    {
        if (item.Visible && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record EngagementSlotLabelLengthRule(int MaxLength) : IEngagementSlotRule
{
    public List<string> Check(EngagementSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementSlotCapacityRangeRule(int Min, int Max) : IEngagementSlotRule
{
    public List<string> Check(EngagementSlot item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a engagement slot.</summary>
public sealed record EngagementSlotValidator(List<IEngagementSlotRule> Rules)
{
    public List<string> Validate(EngagementSlot item)
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

    public void RequireValid(EngagementSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementSlot> items)
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

/// <summary>Functions of engagement slot rules.</summary>
public static class EngagementSlotRules
{
    public static EngagementSlotValidator DefaultEngagementSlotValidator()
    {
        return new EngagementSlotValidator(
            new List<IEngagementSlotRule>
            {
                new EngagementSlotWeightRangeRule(2.4, 72.1),
                new EngagementSlotVisibleGuardRule(80),
                new EngagementSlotLabelLengthRule(80),
                new EngagementSlotCapacityRangeRule(32, 120),
            });
    }
}
