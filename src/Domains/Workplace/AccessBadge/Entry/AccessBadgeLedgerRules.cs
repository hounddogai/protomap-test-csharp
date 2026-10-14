namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>A rule a access badge ledger must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeLedgerRule
{
    List<string> Check(AccessBadgeLedger item);
}

public sealed record AccessBadgeLedgerRevisionRangeRule(int Min, int Max) : IAccessBadgeLedgerRule
{
    public List<string> Check(AccessBadgeLedger item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeLedgerSourceLengthRule(int MaxLength) : IAccessBadgeLedgerRule
{
    public List<string> Check(AccessBadgeLedger item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeLedgerCodePrefixRule(string Prefix) : IAccessBadgeLedgerRule
{
    public List<string> Check(AccessBadgeLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeLedgerRequiredLabelRule(string Message) : IAccessBadgeLedgerRule
{
    public List<string> Check(AccessBadgeLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge ledger.</summary>
public sealed record AccessBadgeLedgerValidator(List<IAccessBadgeLedgerRule> Rules)
{
    public List<string> Validate(AccessBadgeLedger item)
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

    public void RequireValid(AccessBadgeLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeLedger> items)
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

/// <summary>Functions of access badge ledger rules.</summary>
public static class AccessBadgeLedgerRules
{
    public static AccessBadgeLedgerValidator DefaultAccessBadgeLedgerValidator()
    {
        return new AccessBadgeLedgerValidator(
            new List<IAccessBadgeLedgerRule>
            {
                new AccessBadgeLedgerRevisionRangeRule(64, 255),
                new AccessBadgeLedgerSourceLengthRule(16),
                new AccessBadgeLedgerCodePrefixRule("ACM"),
                new AccessBadgeLedgerRequiredLabelRule("access badge ledger needs this field"),
            });
    }
}
