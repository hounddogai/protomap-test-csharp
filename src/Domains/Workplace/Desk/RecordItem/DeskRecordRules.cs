namespace Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>A rule a desk record must pass. Check returns its problems, or none.</summary>
public interface IDeskRecordRule
{
    List<string> Check(DeskRecord item);
}

public sealed record DeskRecordRequiredCodeRule(string Message) : IDeskRecordRule
{
    public List<string> Check(DeskRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskRecordCodePrefixRule(string Prefix) : IDeskRecordRule
{
    public List<string> Check(DeskRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRecordCodeLengthRule(int MaxLength) : IDeskRecordRule
{
    public List<string> Check(DeskRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRecordOwnerIdLengthRule(int MaxLength) : IDeskRecordRule
{
    public List<string> Check(DeskRecord item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRecordRequiredTitleRule(string Message) : IDeskRecordRule
{
    public List<string> Check(DeskRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk record.</summary>
public sealed record DeskRecordValidator(List<IDeskRecordRule> Rules)
{
    public List<string> Validate(DeskRecord item)
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

    public void RequireValid(DeskRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskRecord> items)
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

/// <summary>Functions of desk record rules.</summary>
public static class DeskRecordRules
{
    public static DeskRecordValidator DefaultDeskRecordValidator()
    {
        return new DeskRecordValidator(
            new List<IDeskRecordRule>
            {
                new DeskRecordRequiredCodeRule("desk record needs an entry"),
                new DeskRecordCodePrefixRule("X"),
                new DeskRecordCodeLengthRule(64),
                new DeskRecordOwnerIdLengthRule(1000),
                new DeskRecordRequiredTitleRule("desk record needs an entry"),
            });
    }
}
