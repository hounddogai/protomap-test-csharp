namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceClaimManager(IBalanceClaimStore Store, BalanceClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of balance claim workflow.</summary>
public static class BalanceClaimWorkflow
{
    public static BalanceClaimManager NewBalanceClaimManager(IBalanceClaimStore store, IClock clock)
    {
        return new BalanceClaimManager(
            store,
            BalanceClaimRules.DefaultBalanceClaimValidator(),
            clock,
            new List<string>());
    }
}
