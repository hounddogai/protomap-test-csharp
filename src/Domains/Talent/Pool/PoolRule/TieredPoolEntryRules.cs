namespace Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>A rule a tiered pool entry must pass. Check returns its problems, or none.</summary>
public interface ITieredPoolEntryRule
{
    List<string> Check(TieredPoolEntry item);
}

public sealed record TieredPoolEntryRequiredNameRule(string Message) : ITieredPoolEntryRule
{
    public List<string> Check(TieredPoolEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolEntryRequiredNotesRule(string Message) : ITieredPoolEntryRule
{
    public List<string> Check(TieredPoolEntry item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolEntryCodeLengthRule(int MaxLength) : ITieredPoolEntryRule
{
    public List<string> Check(TieredPoolEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pool entry.</summary>
public sealed record TieredPoolEntryValidator(List<ITieredPoolEntryRule> Rules)
{
    public List<string> Validate(TieredPoolEntry item)
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

    public void RequireValid(TieredPoolEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPoolEntry> items)
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

/// <summary>Functions of tiered pool entry rules.</summary>
public static class TieredPoolEntryRules
{
    public static TieredPoolEntryValidator DefaultTieredPoolEntryValidator()
    {
        return new TieredPoolEntryValidator(
            new List<ITieredPoolEntryRule>
            {
                new TieredPoolEntryRequiredNameRule("tiered pool entry needs this field"),
                new TieredPoolEntryRequiredNotesRule("tiered pool entry needs this field"),
                new TieredPoolEntryCodeLengthRule(120),
            });
    }
}
