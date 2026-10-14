namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualBaselineManager(IAccrualBaselineStore Store, AccrualBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of accrual baseline workflow.</summary>
public static class AccrualBaselineWorkflow
{
    public static AccrualBaselineManager NewAccrualBaselineManager(IAccrualBaselineStore store, IClock clock)
    {
        return new AccrualBaselineManager(
            store,
            AccrualBaselineRules.DefaultAccrualBaselineValidator(),
            clock,
            new List<string>());
    }
}
