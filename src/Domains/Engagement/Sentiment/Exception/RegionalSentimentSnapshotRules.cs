namespace Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>A rule a regional sentiment snapshot must pass. Check returns its problems, or none.</summary>
public interface IRegionalSentimentSnapshotRule
{
    List<string> Check(RegionalSentimentSnapshot item);
}

public sealed record RegionalSentimentSnapshotEmployeeIdLengthRule(int MaxLength) : IRegionalSentimentSnapshotRule
{
    public List<string> Check(RegionalSentimentSnapshot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentSnapshotCodePrefixRule(string Prefix) : IRegionalSentimentSnapshotRule
{
    public List<string> Check(RegionalSentimentSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentSnapshotRequiredLabelRule(string Message) : IRegionalSentimentSnapshotRule
{
    public List<string> Check(RegionalSentimentSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSentimentSnapshotRequiredEmployeeIdRule(string Message) : IRegionalSentimentSnapshotRule
{
    public List<string> Check(RegionalSentimentSnapshot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional sentiment snapshot.</summary>
public sealed record RegionalSentimentSnapshotValidator(List<IRegionalSentimentSnapshotRule> Rules)
{
    public List<string> Validate(RegionalSentimentSnapshot item)
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

    public void RequireValid(RegionalSentimentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSentimentSnapshot> items)
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

/// <summary>Functions of regional sentiment snapshot rules.</summary>
public static class RegionalSentimentSnapshotRules
{
    public static RegionalSentimentSnapshotValidator DefaultRegionalSentimentSnapshotValidator()
    {
        return new RegionalSentimentSnapshotValidator(
            new List<IRegionalSentimentSnapshotRule>
            {
                new RegionalSentimentSnapshotEmployeeIdLengthRule(80),
                new RegionalSentimentSnapshotCodePrefixRule("ACM"),
                new RegionalSentimentSnapshotRequiredLabelRule("regional sentiment snapshot needs this field"),
                new RegionalSentimentSnapshotRequiredEmployeeIdRule("regional sentiment snapshot needs a value"),
            });
    }
}
