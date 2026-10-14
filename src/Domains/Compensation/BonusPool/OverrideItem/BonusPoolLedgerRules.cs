namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>A rule a bonus pool ledger must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolLedgerRule
{
    List<string> Check(BonusPoolLedger item);
}

public sealed record BonusPoolLedgerRequiredDescriptionRule(string Message) : IBonusPoolLedgerRule
{
    public List<string> Check(BonusPoolLedger item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolLedgerCodePrefixRule(string Prefix) : IBonusPoolLedgerRule
{
    public List<string> Check(BonusPoolLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolLedgerDescriptionLengthRule(int MaxLength) : IBonusPoolLedgerRule
{
    public List<string> Check(BonusPoolLedger item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolLedgerRequiredCodeRule(string Message) : IBonusPoolLedgerRule
{
    public List<string> Check(BonusPoolLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolLedgerCodeLengthRule(int MaxLength) : IBonusPoolLedgerRule
{
    public List<string> Check(BonusPoolLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool ledger.</summary>
public sealed record BonusPoolLedgerValidator(List<IBonusPoolLedgerRule> Rules)
{
    public List<string> Validate(BonusPoolLedger item)
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

    public void RequireValid(BonusPoolLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolLedger> items)
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

/// <summary>Functions of bonus pool ledger rules.</summary>
public static class BonusPoolLedgerRules
{
    public static BonusPoolLedgerValidator DefaultBonusPoolLedgerValidator()
    {
        return new BonusPoolLedgerValidator(
            new List<IBonusPoolLedgerRule>
            {
                new BonusPoolLedgerRequiredDescriptionRule("bonus pool ledger needs an entry"),
                new BonusPoolLedgerCodePrefixRule("ACM"),
                new BonusPoolLedgerDescriptionLengthRule(120),
                new BonusPoolLedgerRequiredCodeRule("bonus pool ledger needs a value"),
                new BonusPoolLedgerCodeLengthRule(32),
            });
    }
}
