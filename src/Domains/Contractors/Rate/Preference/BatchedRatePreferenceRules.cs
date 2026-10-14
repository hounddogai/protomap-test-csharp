namespace Acme.Workforce.Contractors.Rate.Preference;

/// <summary>A rule a batched rate preference must pass. Check returns its problems, or none.</summary>
public interface IBatchedRatePreferenceRule
{
    List<string> Check(BatchedRatePreference item);
}

public sealed record BatchedRatePreferenceRequiredSegmentRule(string Message) : IBatchedRatePreferenceRule
{
    public List<string> Check(BatchedRatePreference item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRatePreferenceSegmentLengthRule(int MaxLength) : IBatchedRatePreferenceRule
{
    public List<string> Check(BatchedRatePreference item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRatePreferenceRequiredTitleRule(string Message) : IBatchedRatePreferenceRule
{
    public List<string> Check(BatchedRatePreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched rate preference.</summary>
public sealed record BatchedRatePreferenceValidator(List<IBatchedRatePreferenceRule> Rules)
{
    public List<string> Validate(BatchedRatePreference item)
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

    public void RequireValid(BatchedRatePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRatePreference> items)
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

/// <summary>Functions of batched rate preference rules.</summary>
public static class BatchedRatePreferenceRules
{
    public static BatchedRatePreferenceValidator DefaultBatchedRatePreferenceValidator()
    {
        return new BatchedRatePreferenceValidator(
            new List<IBatchedRatePreferenceRule>
            {
                new BatchedRatePreferenceRequiredSegmentRule("batched rate preference needs a value"),
                new BatchedRatePreferenceSegmentLengthRule(64),
                new BatchedRatePreferenceRequiredTitleRule("batched rate preference needs a value"),
            });
    }
}
