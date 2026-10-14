namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary balance metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryBalanceMetricManager(IPrimaryBalanceMetricStore Store, PrimaryBalanceMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary balance metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryBalanceMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary balance metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryBalanceMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of primary balance metric workflow.</summary>
public static class PrimaryBalanceMetricWorkflow
{
    public static PrimaryBalanceMetricManager NewPrimaryBalanceMetricManager(IPrimaryBalanceMetricStore store, IClock clock)
    {
        return new PrimaryBalanceMetricManager(
            store,
            PrimaryBalanceMetricRules.DefaultPrimaryBalanceMetricValidator(),
            clock,
            new List<string>());
    }
}
