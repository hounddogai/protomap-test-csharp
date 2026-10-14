namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending leave request batch must pass. Check returns its problems, or none.</summary>
public interface IPendingLeaveRequestBatchRule
{
    List<string> Check(PendingLeaveRequestBatch item);
}

public sealed record PendingLeaveRequestBatchRequiredCodeRule(string Message) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLeaveRequestBatchEndsAtRangeRule(int Min, int Max) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLeaveRequestBatchFloorValueRangeRule(double Min, double Max) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLeaveRequestBatchAllowedTitleRule(List<string> Allowed) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingLeaveRequestBatchCodePrefixRule(string Prefix) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingLeaveRequestBatchRequiredTitleRule(string Message) : IPendingLeaveRequestBatchRule
{
    public List<string> Check(PendingLeaveRequestBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending leave request batch.</summary>
public sealed record PendingLeaveRequestBatchValidator(List<IPendingLeaveRequestBatchRule> Rules)
{
    public List<string> Validate(PendingLeaveRequestBatch item)
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

    public void RequireValid(PendingLeaveRequestBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLeaveRequestBatch> items)
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

/// <summary>Functions of pending leave request batch rules.</summary>
public static class PendingLeaveRequestBatchRules
{
    public static PendingLeaveRequestBatchValidator DefaultPendingLeaveRequestBatchValidator()
    {
        return new PendingLeaveRequestBatchValidator(
            new List<IPendingLeaveRequestBatchRule>
            {
                new PendingLeaveRequestBatchRequiredCodeRule("pending leave request batch needs a value"),
                new PendingLeaveRequestBatchEndsAtRangeRule(16, 255),
                new PendingLeaveRequestBatchFloorValueRangeRule(3.2, 952.6),
                new PendingLeaveRequestBatchAllowedTitleRule(new List<string> { "batched", "legacy", "batched" }),
                new PendingLeaveRequestBatchCodePrefixRule("X"),
                new PendingLeaveRequestBatchRequiredTitleRule("pending leave request batch needs this field"),
            });
    }
}
