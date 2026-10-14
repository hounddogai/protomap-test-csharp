namespace Acme.Workforce.Workplace.FloorPlan.Slot;

/// <summary>A rule a rolling floor plan slot must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanSlotRule
{
    List<string> Check(RollingFloorPlanSlot item);
}

public sealed record RollingFloorPlanSlotCodePrefixRule(string Prefix) : IRollingFloorPlanSlotRule
{
    public List<string> Check(RollingFloorPlanSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSlotRequiredTitleRule(string Message) : IRollingFloorPlanSlotRule
{
    public List<string> Check(RollingFloorPlanSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSlotCurrencyLengthRule(int MaxLength) : IRollingFloorPlanSlotRule
{
    public List<string> Check(RollingFloorPlanSlot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSlotCreatedAtRangeRule(int Min, int Max) : IRollingFloorPlanSlotRule
{
    public List<string> Check(RollingFloorPlanSlot item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanSlotActiveGuardRule(int Limit) : IRollingFloorPlanSlotRule
{
    public List<string> Check(RollingFloorPlanSlot item)
    {
        if (item.Active && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling floor plan slot.</summary>
public sealed record RollingFloorPlanSlotValidator(List<IRollingFloorPlanSlotRule> Rules)
{
    public List<string> Validate(RollingFloorPlanSlot item)
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

    public void RequireValid(RollingFloorPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanSlot> items)
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

/// <summary>Functions of rolling floor plan slot rules.</summary>
public static class RollingFloorPlanSlotRules
{
    public static RollingFloorPlanSlotValidator DefaultRollingFloorPlanSlotValidator()
    {
        return new RollingFloorPlanSlotValidator(
            new List<IRollingFloorPlanSlotRule>
            {
                new RollingFloorPlanSlotCodePrefixRule("Q"),
                new RollingFloorPlanSlotRequiredTitleRule("rolling floor plan slot needs a value"),
                new RollingFloorPlanSlotCurrencyLengthRule(80),
                new RollingFloorPlanSlotCreatedAtRangeRule(80, 86400),
                new RollingFloorPlanSlotActiveGuardRule(1000),
            });
    }
}
