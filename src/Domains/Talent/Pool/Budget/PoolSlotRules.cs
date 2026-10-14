namespace Acme.Workforce.Talent.Pool.Budget;

/// <summary>A rule a pool slot must pass. Check returns its problems, or none.</summary>
public interface IPoolSlotRule
{
    List<string> Check(PoolSlot item);
}

public sealed record PoolSlotRequiredCodeRule(string Message) : IPoolSlotRule
{
    public List<string> Check(PoolSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolSlotLabelLengthRule(int MaxLength) : IPoolSlotRule
{
    public List<string> Check(PoolSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolSlotFloorValueRangeRule(double Min, double Max) : IPoolSlotRule
{
    public List<string> Check(PoolSlot item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolSlotCodePrefixRule(string Prefix) : IPoolSlotRule
{
    public List<string> Check(PoolSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolSlotRequiredManagerIdRule(string Message) : IPoolSlotRule
{
    public List<string> Check(PoolSlot item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool slot.</summary>
public sealed record PoolSlotValidator(List<IPoolSlotRule> Rules)
{
    public List<string> Validate(PoolSlot item)
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

    public void RequireValid(PoolSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolSlot> items)
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

/// <summary>Functions of pool slot rules.</summary>
public static class PoolSlotRules
{
    public static PoolSlotValidator DefaultPoolSlotValidator()
    {
        return new PoolSlotValidator(
            new List<IPoolSlotRule>
            {
                new PoolSlotRequiredCodeRule("pool slot needs this field"),
                new PoolSlotLabelLengthRule(64),
                new PoolSlotFloorValueRangeRule(3.0, 972.0),
                new PoolSlotCodePrefixRule("ACM"),
                new PoolSlotRequiredManagerIdRule("pool slot needs a value"),
            });
    }
}
