namespace Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>A rule a pending merit cycle batch must pass. Check returns its problems, or none.</summary>
public interface IPendingMeritCycleBatchRule
{
    List<string> Check(PendingMeritCycleBatch item);
}

public sealed record PendingMeritCycleBatchRequiredTitleRule(string Message) : IPendingMeritCycleBatchRule
{
    public List<string> Check(PendingMeritCycleBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingMeritCycleBatchPinnedGuardRule(double Limit) : IPendingMeritCycleBatchRule
{
    public List<string> Check(PendingMeritCycleBatch item)
    {
        if (item.Pinned && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PendingMeritCycleBatchRegionLengthRule(int MaxLength) : IPendingMeritCycleBatchRule
{
    public List<string> Check(PendingMeritCycleBatch item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingMeritCycleBatchTitleLengthRule(int MaxLength) : IPendingMeritCycleBatchRule
{
    public List<string> Check(PendingMeritCycleBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending merit cycle batch.</summary>
public sealed record PendingMeritCycleBatchValidator(List<IPendingMeritCycleBatchRule> Rules)
{
    public List<string> Validate(PendingMeritCycleBatch item)
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

    public void RequireValid(PendingMeritCycleBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingMeritCycleBatch> items)
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

/// <summary>Functions of pending merit cycle batch rules.</summary>
public static class PendingMeritCycleBatchRules
{
    public static PendingMeritCycleBatchValidator DefaultPendingMeritCycleBatchValidator()
    {
        return new PendingMeritCycleBatchValidator(
            new List<IPendingMeritCycleBatchRule>
            {
                new PendingMeritCycleBatchRequiredTitleRule("pending merit cycle batch needs an entry"),
                new PendingMeritCycleBatchPinnedGuardRule(4.0),
                new PendingMeritCycleBatchRegionLengthRule(255),
                new PendingMeritCycleBatchTitleLengthRule(64),
            });
    }
}
