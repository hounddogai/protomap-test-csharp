namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped visitor batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedVisitorBatchDesk(ICappedVisitorBatchStore Store, CappedVisitorBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped visitor batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedVisitorBatchStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped visitor batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedVisitorBatchStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of capped visitor batch workflow.</summary>
public static class CappedVisitorBatchWorkflow
{
    public static CappedVisitorBatchDesk NewCappedVisitorBatchDesk(ICappedVisitorBatchStore store, IClock clock)
    {
        return new CappedVisitorBatchDesk(
            store,
            CappedVisitorBatchRules.DefaultCappedVisitorBatchValidator(),
            clock,
            new List<string>());
    }
}
