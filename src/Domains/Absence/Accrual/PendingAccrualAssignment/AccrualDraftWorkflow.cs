namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualDraftWorkflow(IAccrualDraftStore Store, AccrualDraftValidator Validator, IClock Clock, List<string> Journal)
{
    public static AccrualDraftWorkflow NewAccrualDraftWorkflow(IAccrualDraftStore store, IClock clock)
    {
        return new AccrualDraftWorkflow(
            store,
            AccrualDraftRules.DefaultAccrualDraftValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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
