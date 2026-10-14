namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending balance templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingBalanceTemplateDesk(IPendingBalanceTemplateStore Store, PendingBalanceTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending balance template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingBalanceTemplateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending balance template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingBalanceTemplateStatus.Withdrawn);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of pending balance template workflow.</summary>
public static class PendingBalanceTemplateWorkflow
{
    public static PendingBalanceTemplateDesk NewPendingBalanceTemplateDesk(IPendingBalanceTemplateStore store, IClock clock)
    {
        return new PendingBalanceTemplateDesk(
            store,
            PendingBalanceTemplateRules.DefaultPendingBalanceTemplateValidator(),
            clock,
            new List<string>());
    }
}
