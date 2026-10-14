namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict return plan batch must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanBatchRule
{
    List<string> Check(StrictReturnPlanBatch item);
}

public sealed record StrictReturnPlanBatchAllowedCodeRule(List<string> Allowed) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBatchTitleLengthRule(int MaxLength) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBatchOwnerIdLengthRule(int MaxLength) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBatchRequiredTitleRule(string Message) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBatchCodeLengthRule(int MaxLength) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanBatchRequiredCodeRule(string Message) : IStrictReturnPlanBatchRule
{
    public List<string> Check(StrictReturnPlanBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan batch.</summary>
public sealed record StrictReturnPlanBatchValidator(List<IStrictReturnPlanBatchRule> Rules)
{
    public List<string> Validate(StrictReturnPlanBatch item)
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

    public void RequireValid(StrictReturnPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanBatch> items)
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

/// <summary>Functions of strict return plan batch rules.</summary>
public static class StrictReturnPlanBatchRules
{
    public static StrictReturnPlanBatchValidator DefaultStrictReturnPlanBatchValidator()
    {
        return new StrictReturnPlanBatchValidator(
            new List<IStrictReturnPlanBatchRule>
            {
                new StrictReturnPlanBatchAllowedCodeRule(new List<string> { "pending", "adaptive" }),
                new StrictReturnPlanBatchTitleLengthRule(1000),
                new StrictReturnPlanBatchOwnerIdLengthRule(64),
                new StrictReturnPlanBatchRequiredTitleRule("strict return plan batch needs this field"),
                new StrictReturnPlanBatchCodeLengthRule(86400),
                new StrictReturnPlanBatchRequiredCodeRule("strict return plan batch needs this field"),
            });
    }
}
