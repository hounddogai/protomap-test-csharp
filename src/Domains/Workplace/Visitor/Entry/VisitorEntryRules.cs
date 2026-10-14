namespace Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>A rule a visitor entry must pass. Check returns its problems, or none.</summary>
public interface IVisitorEntryRule
{
    List<string> Check(VisitorEntry item);
}

public sealed record VisitorEntryRevisionRangeRule(int Min, int Max) : IVisitorEntryRule
{
    public List<string> Check(VisitorEntry item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorEntryExpiresAtRangeRule(int Min, int Max) : IVisitorEntryRule
{
    public List<string> Check(VisitorEntry item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorEntryRequiredTitleRule(string Message) : IVisitorEntryRule
{
    public List<string> Check(VisitorEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor entry.</summary>
public sealed record VisitorEntryValidator(List<IVisitorEntryRule> Rules)
{
    public List<string> Validate(VisitorEntry item)
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

    public void RequireValid(VisitorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorEntry> items)
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

/// <summary>Functions of visitor entry rules.</summary>
public static class VisitorEntryRules
{
    public static VisitorEntryValidator DefaultVisitorEntryValidator()
    {
        return new VisitorEntryValidator(
            new List<IVisitorEntryRule>
            {
                new VisitorEntryRevisionRangeRule(1000, 3600),
                new VisitorEntryExpiresAtRangeRule(120, 86400),
                new VisitorEntryRequiredTitleRule("visitor entry needs this field"),
            });
    }
}
