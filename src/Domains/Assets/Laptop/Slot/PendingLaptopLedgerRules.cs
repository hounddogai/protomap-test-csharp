namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending laptop ledger must pass. Check returns its problems, or none.</summary>
public interface IPendingLaptopLedgerRule
{
    List<string> Check(PendingLaptopLedger item);
}

public sealed record PendingLaptopLedgerAllowedNameRule(List<string> Allowed) : IPendingLaptopLedgerRule
{
    public List<string> Check(PendingLaptopLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopLedgerPinnedGuardRule(int Limit) : IPendingLaptopLedgerRule
{
    public List<string> Check(PendingLaptopLedger item)
    {
        if (item.Pinned && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopLedgerRequiredCurrencyRule(string Message) : IPendingLaptopLedgerRule
{
    public List<string> Check(PendingLaptopLedger item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLaptopLedgerRetriesRangeRule(int Min, int Max) : IPendingLaptopLedgerRule
{
    public List<string> Check(PendingLaptopLedger item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending laptop ledger.</summary>
public sealed record PendingLaptopLedgerValidator(List<IPendingLaptopLedgerRule> Rules)
{
    public List<string> Validate(PendingLaptopLedger item)
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

    public void RequireValid(PendingLaptopLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLaptopLedger> items)
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

/// <summary>Functions of pending laptop ledger rules.</summary>
public static class PendingLaptopLedgerRules
{
    public static PendingLaptopLedgerValidator DefaultPendingLaptopLedgerValidator()
    {
        return new PendingLaptopLedgerValidator(
            new List<IPendingLaptopLedgerRule>
            {
                new PendingLaptopLedgerAllowedNameRule(new List<string> { "batched" }),
                new PendingLaptopLedgerPinnedGuardRule(86400),
                new PendingLaptopLedgerRequiredCurrencyRule("pending laptop ledger needs this field"),
                new PendingLaptopLedgerRetriesRangeRule(120, 86400),
            });
    }
}
