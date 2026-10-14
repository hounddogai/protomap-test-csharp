namespace Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>A rule a batched mobility slot must pass. Check returns its problems, or none.</summary>
public interface IBatchedMobilitySlotRule
{
    List<string> Check(BatchedMobilitySlot item);
}

public sealed record BatchedMobilitySlotCodePrefixRule(string Prefix) : IBatchedMobilitySlotRule
{
    public List<string> Check(BatchedMobilitySlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilitySlotCodeLengthRule(int MaxLength) : IBatchedMobilitySlotRule
{
    public List<string> Check(BatchedMobilitySlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilitySlotFloorValueRangeRule(double Min, double Max) : IBatchedMobilitySlotRule
{
    public List<string> Check(BatchedMobilitySlot item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilitySlotQuantityRangeRule(int Min, int Max) : IBatchedMobilitySlotRule
{
    public List<string> Check(BatchedMobilitySlot item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched mobility slot.</summary>
public sealed record BatchedMobilitySlotValidator(List<IBatchedMobilitySlotRule> Rules)
{
    public List<string> Validate(BatchedMobilitySlot item)
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

    public void RequireValid(BatchedMobilitySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedMobilitySlot> items)
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

/// <summary>Functions of batched mobility slot rules.</summary>
public static class BatchedMobilitySlotRules
{
    public static BatchedMobilitySlotValidator DefaultBatchedMobilitySlotValidator()
    {
        return new BatchedMobilitySlotValidator(
            new List<IBatchedMobilitySlotRule>
            {
                new BatchedMobilitySlotCodePrefixRule("REF"),
                new BatchedMobilitySlotCodeLengthRule(80),
                new BatchedMobilitySlotFloorValueRangeRule(3.2, 3047.8),
                new BatchedMobilitySlotQuantityRangeRule(255, 86400),
            });
    }
}
