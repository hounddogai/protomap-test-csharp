namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>A rule a adaptive desk batch must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveDeskBatchRule
{
    List<string> Check(AdaptiveDeskBatch item);
}

public sealed record AdaptiveDeskBatchRequiredTitleRule(string Message) : IAdaptiveDeskBatchRule
{
    public List<string> Check(AdaptiveDeskBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskBatchRetriesRangeRule(int Min, int Max) : IAdaptiveDeskBatchRule
{
    public List<string> Check(AdaptiveDeskBatch item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskBatchRequiredSegmentRule(string Message) : IAdaptiveDeskBatchRule
{
    public List<string> Check(AdaptiveDeskBatch item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskBatchRequiredCodeRule(string Message) : IAdaptiveDeskBatchRule
{
    public List<string> Check(AdaptiveDeskBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive desk batch.</summary>
public sealed record AdaptiveDeskBatchValidator(List<IAdaptiveDeskBatchRule> Rules)
{
    public List<string> Validate(AdaptiveDeskBatch item)
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

    public void RequireValid(AdaptiveDeskBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveDeskBatch> items)
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

/// <summary>Functions of adaptive desk batch rules.</summary>
public static class AdaptiveDeskBatchRules
{
    public static AdaptiveDeskBatchValidator DefaultAdaptiveDeskBatchValidator()
    {
        return new AdaptiveDeskBatchValidator(
            new List<IAdaptiveDeskBatchRule>
            {
                new AdaptiveDeskBatchRequiredTitleRule("adaptive desk batch needs an entry"),
                new AdaptiveDeskBatchRetriesRangeRule(16, 32),
                new AdaptiveDeskBatchRequiredSegmentRule("adaptive desk batch needs this field"),
                new AdaptiveDeskBatchRequiredCodeRule("adaptive desk batch needs this field"),
            });
    }
}
