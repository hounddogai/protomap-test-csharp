namespace Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>A rule a lenient recognition entry must pass. Check returns its problems, or none.</summary>
public interface ILenientRecognitionEntryRule
{
    List<string> Check(LenientRecognitionEntry item);
}

public sealed record LenientRecognitionEntryRequiredDescriptionRule(string Message) : ILenientRecognitionEntryRule
{
    public List<string> Check(LenientRecognitionEntry item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionEntryRequiredSummaryRule(string Message) : ILenientRecognitionEntryRule
{
    public List<string> Check(LenientRecognitionEntry item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionEntryHoursRangeRule(double Min, double Max) : ILenientRecognitionEntryRule
{
    public List<string> Check(LenientRecognitionEntry item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionEntryCodePrefixRule(string Prefix) : ILenientRecognitionEntryRule
{
    public List<string> Check(LenientRecognitionEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient recognition entry.</summary>
public sealed record LenientRecognitionEntryValidator(List<ILenientRecognitionEntryRule> Rules)
{
    public List<string> Validate(LenientRecognitionEntry item)
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

    public void RequireValid(LenientRecognitionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRecognitionEntry> items)
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

/// <summary>Functions of lenient recognition entry rules.</summary>
public static class LenientRecognitionEntryRules
{
    public static LenientRecognitionEntryValidator DefaultLenientRecognitionEntryValidator()
    {
        return new LenientRecognitionEntryValidator(
            new List<ILenientRecognitionEntryRule>
            {
                new LenientRecognitionEntryRequiredDescriptionRule("lenient recognition entry needs an entry"),
                new LenientRecognitionEntryRequiredSummaryRule("lenient recognition entry needs an entry"),
                new LenientRecognitionEntryHoursRangeRule(6.1, 1901.5),
                new LenientRecognitionEntryCodePrefixRule("Q"),
            });
    }
}
