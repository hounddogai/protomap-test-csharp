namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceVarianceManager(IBalanceVarianceStore Store, BalanceVarianceValidator Validator, IClock Clock, List<string> Journal, BalanceVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance variance {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of balance variance workflow.</summary>
public static class BalanceVarianceWorkflow
{
    public static BalanceVarianceManager NewBalanceVarianceManager(IBalanceVarianceStore store, IClock clock)
    {
        return new BalanceVarianceManager(
            store,
            BalanceVarianceRules.DefaultBalanceVarianceValidator(),
            clock,
            new List<string>(),
            BalanceVarianceEngine.DefaultBalanceVarianceEngine());
    }
}
