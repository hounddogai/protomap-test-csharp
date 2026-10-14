namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>A rule a scheduled cost model batch must pass. Check returns its problems, or none.</summary>
public interface IScheduledCostModelBatchRule
{
    List<string> Check(ScheduledCostModelBatch item);
}

public sealed record ScheduledCostModelBatchOwnerIdLengthRule(int MaxLength) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelBatchExpiresAtRangeRule(int Min, int Max) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelBatchNameLengthRule(int MaxLength) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelBatchCapacityRangeRule(int Min, int Max) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
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

public sealed record ScheduledCostModelBatchCodeLengthRule(int MaxLength) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelBatchExternalRefLengthRule(int MaxLength) : IScheduledCostModelBatchRule
{
    public List<string> Check(ScheduledCostModelBatch item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled cost model batch.</summary>
public sealed record ScheduledCostModelBatchValidator(List<IScheduledCostModelBatchRule> Rules)
{
    public List<string> Validate(ScheduledCostModelBatch item)
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

    public void RequireValid(ScheduledCostModelBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledCostModelBatch> items)
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

/// <summary>Functions of scheduled cost model batch rules.</summary>
public static class ScheduledCostModelBatchRules
{
    public static ScheduledCostModelBatchValidator DefaultScheduledCostModelBatchValidator()
    {
        return new ScheduledCostModelBatchValidator(
            new List<IScheduledCostModelBatchRule>
            {
                new ScheduledCostModelBatchOwnerIdLengthRule(32),
                new ScheduledCostModelBatchExpiresAtRangeRule(80, 86400),
                new ScheduledCostModelBatchNameLengthRule(255),
                new ScheduledCostModelBatchCapacityRangeRule(64, 86400),
                new ScheduledCostModelBatchCodeLengthRule(255),
                new ScheduledCostModelBatchExternalRefLengthRule(32),
            });
    }
}
