namespace Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>A rule a batched span preference must pass. Check returns its problems, or none.</summary>
public interface IBatchedSpanPreferenceRule
{
    List<string> Check(BatchedSpanPreference item);
}

public sealed record BatchedSpanPreferencePriorityRangeRule(int Min, int Max) : IBatchedSpanPreferenceRule
{
    public List<string> Check(BatchedSpanPreference item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedSpanPreferenceCodePrefixRule(string Prefix) : IBatchedSpanPreferenceRule
{
    public List<string> Check(BatchedSpanPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSpanPreferenceRequiredNameRule(string Message) : IBatchedSpanPreferenceRule
{
    public List<string> Check(BatchedSpanPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched span preference.</summary>
public sealed record BatchedSpanPreferenceValidator(List<IBatchedSpanPreferenceRule> Rules)
{
    public List<string> Validate(BatchedSpanPreference item)
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

    public void RequireValid(BatchedSpanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSpanPreference> items)
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

/// <summary>Functions of batched span preference rules.</summary>
public static class BatchedSpanPreferenceRules
{
    public static BatchedSpanPreferenceValidator DefaultBatchedSpanPreferenceValidator()
    {
        return new BatchedSpanPreferenceValidator(
            new List<IBatchedSpanPreferenceRule>
            {
                new BatchedSpanPreferencePriorityRangeRule(80, 120),
                new BatchedSpanPreferenceCodePrefixRule("X"),
                new BatchedSpanPreferenceRequiredNameRule("batched span preference needs an entry"),
            });
    }
}
