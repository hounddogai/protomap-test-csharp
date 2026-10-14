namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceTemplateDesk(IBalanceTemplateStore Store, BalanceTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceTemplateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceTemplateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of balance template workflow.</summary>
public static class BalanceTemplateWorkflow
{
    public static BalanceTemplateDesk NewBalanceTemplateDesk(IBalanceTemplateStore store, IClock clock)
    {
        return new BalanceTemplateDesk(
            store,
            BalanceTemplateRules.DefaultBalanceTemplateValidator(),
            clock,
            new List<string>());
    }
}
