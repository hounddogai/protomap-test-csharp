namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched town hall quota must pass. Check returns its problems, or none.</summary>
public interface IBatchedTownHallQuotaRule
{
    List<string> Check(BatchedTownHallQuota item);
}

public sealed record BatchedTownHallQuotaRequiredCodeRule(string Message) : IBatchedTownHallQuotaRule
{
    public List<string> Check(BatchedTownHallQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTownHallQuotaSequenceRangeRule(int Min, int Max) : IBatchedTownHallQuotaRule
{
    public List<string> Check(BatchedTownHallQuota item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedTownHallQuotaAllowedManagerIdRule(List<string> Allowed) : IBatchedTownHallQuotaRule
{
    public List<string> Check(BatchedTownHallQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched town hall quota.</summary>
public sealed record BatchedTownHallQuotaValidator(List<IBatchedTownHallQuotaRule> Rules)
{
    public List<string> Validate(BatchedTownHallQuota item)
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

    public void RequireValid(BatchedTownHallQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTownHallQuota> items)
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

/// <summary>Functions of batched town hall quota rules.</summary>
public static class BatchedTownHallQuotaRules
{
    public static BatchedTownHallQuotaValidator DefaultBatchedTownHallQuotaValidator()
    {
        return new BatchedTownHallQuotaValidator(
            new List<IBatchedTownHallQuotaRule>
            {
                new BatchedTownHallQuotaRequiredCodeRule("batched town hall quota needs this field"),
                new BatchedTownHallQuotaSequenceRangeRule(120, 86400),
                new BatchedTownHallQuotaAllowedManagerIdRule(new List<string>()),
            });
    }
}
