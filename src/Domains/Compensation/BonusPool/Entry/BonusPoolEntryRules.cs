namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool entry must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolEntryRule
{
    List<string> Check(BonusPoolEntry item);
}

public sealed record BonusPoolEntryCodeLengthRule(int MaxLength) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEntryAllowedSummaryRule(List<string> Allowed) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEntryPriorityRangeRule(int Min, int Max) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
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

public sealed record BonusPoolEntryRequiredNotesRule(string Message) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEntrySummaryLengthRule(int MaxLength) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolEntryRequiredNameRule(string Message) : IBonusPoolEntryRule
{
    public List<string> Check(BonusPoolEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool entry.</summary>
public sealed record BonusPoolEntryValidator(List<IBonusPoolEntryRule> Rules)
{
    public List<string> Validate(BonusPoolEntry item)
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

    public void RequireValid(BonusPoolEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolEntry> items)
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

/// <summary>Functions of bonus pool entry rules.</summary>
public static class BonusPoolEntryRules
{
    public static BonusPoolEntryValidator DefaultBonusPoolEntryValidator()
    {
        return new BonusPoolEntryValidator(
            new List<IBonusPoolEntryRule>
            {
                new BonusPoolEntryCodeLengthRule(16),
                new BonusPoolEntryAllowedSummaryRule(new List<string> { "legacy" }),
                new BonusPoolEntryPriorityRangeRule(16, 3600),
                new BonusPoolEntryRequiredNotesRule("bonus pool entry needs an entry"),
                new BonusPoolEntrySummaryLengthRule(86400),
                new BonusPoolEntryRequiredNameRule("bonus pool entry needs this field"),
            });
    }
}
