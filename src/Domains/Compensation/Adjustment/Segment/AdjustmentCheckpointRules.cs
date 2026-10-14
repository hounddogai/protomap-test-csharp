namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>A rule a adjustment checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentCheckpointRule
{
    List<string> Check(AdjustmentCheckpoint item);
}

public sealed record AdjustmentCheckpointRequiredOwnerIdRule(string Message) : IAdjustmentCheckpointRule
{
    public List<string> Check(AdjustmentCheckpoint item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentCheckpointRequiredExternalRefRule(string Message) : IAdjustmentCheckpointRule
{
    public List<string> Check(AdjustmentCheckpoint item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentCheckpointNameLengthRule(int MaxLength) : IAdjustmentCheckpointRule
{
    public List<string> Check(AdjustmentCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment checkpoint.</summary>
public sealed record AdjustmentCheckpointValidator(List<IAdjustmentCheckpointRule> Rules)
{
    public List<string> Validate(AdjustmentCheckpoint item)
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

    public void RequireValid(AdjustmentCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentCheckpoint> items)
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

/// <summary>Functions of adjustment checkpoint rules.</summary>
public static class AdjustmentCheckpointRules
{
    public static AdjustmentCheckpointValidator DefaultAdjustmentCheckpointValidator()
    {
        return new AdjustmentCheckpointValidator(
            new List<IAdjustmentCheckpointRule>
            {
                new AdjustmentCheckpointRequiredOwnerIdRule("adjustment checkpoint needs a value"),
                new AdjustmentCheckpointRequiredExternalRefRule("adjustment checkpoint needs this field"),
                new AdjustmentCheckpointNameLengthRule(16),
            });
    }
}
