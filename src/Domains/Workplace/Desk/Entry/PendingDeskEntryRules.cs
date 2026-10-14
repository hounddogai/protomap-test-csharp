namespace Acme.Workforce.Workplace.Desk.Entry;

/// <summary>A rule a pending desk entry must pass. Check returns its problems, or none.</summary>
public interface IPendingDeskEntryRule
{
    List<string> Check(PendingDeskEntry item);
}

public sealed record PendingDeskEntryDepartmentLengthRule(int MaxLength) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskEntryCodePrefixRule(string Prefix) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskEntryRequiredLabelRule(string Message) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskEntryCodeLengthRule(int MaxLength) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskEntryRequiredUnitRule(string Message) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskEntryRequiredCodeRule(string Message) : IPendingDeskEntryRule
{
    public List<string> Check(PendingDeskEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending desk entry.</summary>
public sealed record PendingDeskEntryValidator(List<IPendingDeskEntryRule> Rules)
{
    public List<string> Validate(PendingDeskEntry item)
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

    public void RequireValid(PendingDeskEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeskEntry> items)
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

/// <summary>Functions of pending desk entry rules.</summary>
public static class PendingDeskEntryRules
{
    public static PendingDeskEntryValidator DefaultPendingDeskEntryValidator()
    {
        return new PendingDeskEntryValidator(
            new List<IPendingDeskEntryRule>
            {
                new PendingDeskEntryDepartmentLengthRule(3600),
                new PendingDeskEntryCodePrefixRule("X"),
                new PendingDeskEntryRequiredLabelRule("pending desk entry needs a value"),
                new PendingDeskEntryCodeLengthRule(64),
                new PendingDeskEntryRequiredUnitRule("pending desk entry needs a value"),
                new PendingDeskEntryRequiredCodeRule("pending desk entry needs an entry"),
            });
    }
}
