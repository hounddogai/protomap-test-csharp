namespace Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>A rule a manual recognition segment must pass. Check returns its problems, or none.</summary>
public interface IManualRecognitionSegmentRule
{
    List<string> Check(ManualRecognitionSegment item);
}

public sealed record ManualRecognitionSegmentRequiredTitleRule(string Message) : IManualRecognitionSegmentRule
{
    public List<string> Check(ManualRecognitionSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionSegmentCodePrefixRule(string Prefix) : IManualRecognitionSegmentRule
{
    public List<string> Check(ManualRecognitionSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionSegmentNameLengthRule(int MaxLength) : IManualRecognitionSegmentRule
{
    public List<string> Check(ManualRecognitionSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionSegmentRequiredCodeRule(string Message) : IManualRecognitionSegmentRule
{
    public List<string> Check(ManualRecognitionSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual recognition segment.</summary>
public sealed record ManualRecognitionSegmentValidator(List<IManualRecognitionSegmentRule> Rules)
{
    public List<string> Validate(ManualRecognitionSegment item)
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

    public void RequireValid(ManualRecognitionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRecognitionSegment> items)
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

/// <summary>Functions of manual recognition segment rules.</summary>
public static class ManualRecognitionSegmentRules
{
    public static ManualRecognitionSegmentValidator DefaultManualRecognitionSegmentValidator()
    {
        return new ManualRecognitionSegmentValidator(
            new List<IManualRecognitionSegmentRule>
            {
                new ManualRecognitionSegmentRequiredTitleRule("manual recognition segment needs a value"),
                new ManualRecognitionSegmentCodePrefixRule("ACM"),
                new ManualRecognitionSegmentNameLengthRule(1000),
                new ManualRecognitionSegmentRequiredCodeRule("manual recognition segment needs an entry"),
            });
    }
}
