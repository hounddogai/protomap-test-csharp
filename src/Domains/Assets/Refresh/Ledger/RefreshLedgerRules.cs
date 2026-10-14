namespace Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>A rule a refresh ledger must pass. Check returns its problems, or none.</summary>
public interface IRefreshLedgerRule
{
    List<string> Check(RefreshLedger item);
}

public sealed record RefreshLedgerCodePrefixRule(string Prefix) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshLedgerLabelLengthRule(int MaxLength) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshLedgerArchivedGuardRule(double Limit) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        if (item.Archived && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record RefreshLedgerRequiredCategoryRule(string Message) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshLedgerUnitLengthRule(int MaxLength) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshLedgerRequiredCodeRule(string Message) : IRefreshLedgerRule
{
    public List<string> Check(RefreshLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh ledger.</summary>
public sealed record RefreshLedgerValidator(List<IRefreshLedgerRule> Rules)
{
    public List<string> Validate(RefreshLedger item)
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

    public void RequireValid(RefreshLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshLedger> items)
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

/// <summary>Functions of refresh ledger rules.</summary>
public static class RefreshLedgerRules
{
    public static RefreshLedgerValidator DefaultRefreshLedgerValidator()
    {
        return new RefreshLedgerValidator(
            new List<IRefreshLedgerRule>
            {
                new RefreshLedgerCodePrefixRule("X"),
                new RefreshLedgerLabelLengthRule(32),
                new RefreshLedgerArchivedGuardRule(1.6),
                new RefreshLedgerRequiredCategoryRule("refresh ledger needs an entry"),
                new RefreshLedgerUnitLengthRule(255),
                new RefreshLedgerRequiredCodeRule("refresh ledger needs this field"),
            });
    }
}
