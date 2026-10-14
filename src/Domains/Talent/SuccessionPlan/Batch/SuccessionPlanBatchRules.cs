namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>A rule a succession plan batch must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanBatchRule
{
    List<string> Check(SuccessionPlanBatch item);
}

public sealed record SuccessionPlanBatchRequiredTitleRule(string Message) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBatchTitleLengthRule(int MaxLength) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBatchActiveGuardRule(int Limit) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        if (item.Active && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBatchChannelLengthRule(int MaxLength) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBatchRequiredCodeRule(string Message) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBatchRegionLengthRule(int MaxLength) : ISuccessionPlanBatchRule
{
    public List<string> Check(SuccessionPlanBatch item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan batch.</summary>
public sealed record SuccessionPlanBatchValidator(List<ISuccessionPlanBatchRule> Rules)
{
    public List<string> Validate(SuccessionPlanBatch item)
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

    public void RequireValid(SuccessionPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanBatch> items)
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

/// <summary>Functions of succession plan batch rules.</summary>
public static class SuccessionPlanBatchRules
{
    public static SuccessionPlanBatchValidator DefaultSuccessionPlanBatchValidator()
    {
        return new SuccessionPlanBatchValidator(
            new List<ISuccessionPlanBatchRule>
            {
                new SuccessionPlanBatchRequiredTitleRule("succession plan batch needs this field"),
                new SuccessionPlanBatchTitleLengthRule(3600),
                new SuccessionPlanBatchActiveGuardRule(16),
                new SuccessionPlanBatchChannelLengthRule(80),
                new SuccessionPlanBatchRequiredCodeRule("succession plan batch needs this field"),
                new SuccessionPlanBatchRegionLengthRule(80),
            });
    }
}
