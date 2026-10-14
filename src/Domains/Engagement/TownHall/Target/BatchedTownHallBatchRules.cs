namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched town hall batch must pass. Check returns its problems, or none.</summary>
public interface IBatchedTownHallBatchRule
{
    List<string> Check(BatchedTownHallBatch item);
}

public sealed record BatchedTownHallBatchLabelLengthRule(int MaxLength) : IBatchedTownHallBatchRule
{
    public List<string> Check(BatchedTownHallBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallBatchBadgeLengthRule(int MaxLength) : IBatchedTownHallBatchRule
{
    public List<string> Check(BatchedTownHallBatch item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallBatchAllowedPhoneRule(List<string> Allowed) : IBatchedTownHallBatchRule
{
    public List<string> Check(BatchedTownHallBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallBatchCodeLengthRule(int MaxLength) : IBatchedTownHallBatchRule
{
    public List<string> Check(BatchedTownHallBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched town hall batch.</summary>
public sealed record BatchedTownHallBatchValidator(List<IBatchedTownHallBatchRule> Rules)
{
    public List<string> Validate(BatchedTownHallBatch item)
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

    public void RequireValid(BatchedTownHallBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTownHallBatch> items)
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

/// <summary>Functions of batched town hall batch rules.</summary>
public static class BatchedTownHallBatchRules
{
    public static BatchedTownHallBatchValidator DefaultBatchedTownHallBatchValidator()
    {
        return new BatchedTownHallBatchValidator(
            new List<IBatchedTownHallBatchRule>
            {
                new BatchedTownHallBatchLabelLengthRule(120),
                new BatchedTownHallBatchBadgeLengthRule(255),
                new BatchedTownHallBatchAllowedPhoneRule(new List<string> { "fallback", "default", "regional" }),
                new BatchedTownHallBatchCodeLengthRule(16),
            });
    }
}
