namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>A rule a pending floor plan target must pass. Check returns its problems, or none.</summary>
public interface IPendingFloorPlanTargetRule
{
    List<string> Check(PendingFloorPlanTarget item);
}

public sealed record PendingFloorPlanTargetLabelLengthRule(int MaxLength) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingFloorPlanTargetRequiredLabelRule(string Message) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingFloorPlanTargetRequiredDescriptionRule(string Message) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingFloorPlanTargetCodePrefixRule(string Prefix) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingFloorPlanTargetDescriptionLengthRule(int MaxLength) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingFloorPlanTargetWindowDaysRangeRule(int Min, int Max) : IPendingFloorPlanTargetRule
{
    public List<string> Check(PendingFloorPlanTarget item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending floor plan target.</summary>
public sealed record PendingFloorPlanTargetValidator(List<IPendingFloorPlanTargetRule> Rules)
{
    public List<string> Validate(PendingFloorPlanTarget item)
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

    public void RequireValid(PendingFloorPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingFloorPlanTarget> items)
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

/// <summary>Functions of pending floor plan target rules.</summary>
public static class PendingFloorPlanTargetRules
{
    public static PendingFloorPlanTargetValidator DefaultPendingFloorPlanTargetValidator()
    {
        return new PendingFloorPlanTargetValidator(
            new List<IPendingFloorPlanTargetRule>
            {
                new PendingFloorPlanTargetLabelLengthRule(120),
                new PendingFloorPlanTargetRequiredLabelRule("pending floor plan target needs this field"),
                new PendingFloorPlanTargetRequiredDescriptionRule("pending floor plan target needs an entry"),
                new PendingFloorPlanTargetCodePrefixRule("X"),
                new PendingFloorPlanTargetDescriptionLengthRule(255),
                new PendingFloorPlanTargetWindowDaysRangeRule(32, 3600),
            });
    }
}
