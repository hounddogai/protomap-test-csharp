namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>A rule a manual visitor entry must pass. Check returns its problems, or none.</summary>
public interface IManualVisitorEntryRule
{
    List<string> Check(ManualVisitorEntry item);
}

public sealed record ManualVisitorEntryLabelLengthRule(int MaxLength) : IManualVisitorEntryRule
{
    public List<string> Check(ManualVisitorEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVisitorEntryMinimumRangeRule(double Min, double Max) : IManualVisitorEntryRule
{
    public List<string> Check(ManualVisitorEntry item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVisitorEntryRequiredLabelRule(string Message) : IManualVisitorEntryRule
{
    public List<string> Check(ManualVisitorEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualVisitorEntryCodePrefixRule(string Prefix) : IManualVisitorEntryRule
{
    public List<string> Check(ManualVisitorEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual visitor entry.</summary>
public sealed record ManualVisitorEntryValidator(List<IManualVisitorEntryRule> Rules)
{
    public List<string> Validate(ManualVisitorEntry item)
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

    public void RequireValid(ManualVisitorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualVisitorEntry> items)
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

/// <summary>Functions of manual visitor entry rules.</summary>
public static class ManualVisitorEntryRules
{
    public static ManualVisitorEntryValidator DefaultManualVisitorEntryValidator()
    {
        return new ManualVisitorEntryValidator(
            new List<IManualVisitorEntryRule>
            {
                new ManualVisitorEntryLabelLengthRule(3600),
                new ManualVisitorEntryMinimumRangeRule(7.5, 4123.5),
                new ManualVisitorEntryRequiredLabelRule("manual visitor entry needs a value"),
                new ManualVisitorEntryCodePrefixRule("Q"),
            });
    }
}
