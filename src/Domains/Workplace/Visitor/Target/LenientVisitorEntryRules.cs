namespace Acme.Workforce.Workplace.Visitor.Target;

/// <summary>A rule a lenient visitor entry must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorEntryRule
{
    List<string> Check(LenientVisitorEntry item);
}

public sealed record LenientVisitorEntryRequiredCodeRule(string Message) : ILenientVisitorEntryRule
{
    public List<string> Check(LenientVisitorEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorEntryCodePrefixRule(string Prefix) : ILenientVisitorEntryRule
{
    public List<string> Check(LenientVisitorEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorEntryRequiredNameRule(string Message) : ILenientVisitorEntryRule
{
    public List<string> Check(LenientVisitorEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorEntryThresholdRangeRule(double Min, double Max) : ILenientVisitorEntryRule
{
    public List<string> Check(LenientVisitorEntry item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor entry.</summary>
public sealed record LenientVisitorEntryValidator(List<ILenientVisitorEntryRule> Rules)
{
    public List<string> Validate(LenientVisitorEntry item)
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

    public void RequireValid(LenientVisitorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorEntry> items)
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

/// <summary>Functions of lenient visitor entry rules.</summary>
public static class LenientVisitorEntryRules
{
    public static LenientVisitorEntryValidator DefaultLenientVisitorEntryValidator()
    {
        return new LenientVisitorEntryValidator(
            new List<ILenientVisitorEntryRule>
            {
                new LenientVisitorEntryRequiredCodeRule("lenient visitor entry needs a value"),
                new LenientVisitorEntryCodePrefixRule("X"),
                new LenientVisitorEntryRequiredNameRule("lenient visitor entry needs this field"),
                new LenientVisitorEntryThresholdRangeRule(5.1, 3545.7),
            });
    }
}
