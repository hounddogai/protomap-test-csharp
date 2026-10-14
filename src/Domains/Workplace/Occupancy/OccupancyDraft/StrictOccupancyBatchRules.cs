namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict occupancy batch must pass. Check returns its problems, or none.</summary>
public interface IStrictOccupancyBatchRule
{
    List<string> Check(StrictOccupancyBatch item);
}

public sealed record StrictOccupancyBatchRequiredCodeRule(string Message) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyBatchAllowedTitleRule(List<string> Allowed) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyBatchOwnerIdLengthRule(int MaxLength) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyBatchCodePrefixRule(string Prefix) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyBatchThresholdRangeRule(double Min, double Max) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictOccupancyBatchPinnedGuardRule(int Limit) : IStrictOccupancyBatchRule
{
    public List<string> Check(StrictOccupancyBatch item)
    {
        if (item.Pinned && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict occupancy batch.</summary>
public sealed record StrictOccupancyBatchValidator(List<IStrictOccupancyBatchRule> Rules)
{
    public List<string> Validate(StrictOccupancyBatch item)
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

    public void RequireValid(StrictOccupancyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOccupancyBatch> items)
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

/// <summary>Functions of strict occupancy batch rules.</summary>
public static class StrictOccupancyBatchRules
{
    public static StrictOccupancyBatchValidator DefaultStrictOccupancyBatchValidator()
    {
        return new StrictOccupancyBatchValidator(
            new List<IStrictOccupancyBatchRule>
            {
                new StrictOccupancyBatchRequiredCodeRule("strict occupancy batch needs a value"),
                new StrictOccupancyBatchAllowedTitleRule(new List<string> { "capped", "scheduled" }),
                new StrictOccupancyBatchOwnerIdLengthRule(120),
                new StrictOccupancyBatchCodePrefixRule("ACM"),
                new StrictOccupancyBatchThresholdRangeRule(7.0, 2836.9),
                new StrictOccupancyBatchPinnedGuardRule(64),
            });
    }
}
