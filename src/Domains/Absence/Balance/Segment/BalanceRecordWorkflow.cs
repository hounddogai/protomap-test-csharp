namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance records through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceRecordDesk(IBalanceRecordStore Store, BalanceRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceRecordStatus.Stopped);
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

/// <summary>Functions of balance record workflow.</summary>
public static class BalanceRecordWorkflow
{
    public static BalanceRecordDesk NewBalanceRecordDesk(IBalanceRecordStore store, IClock clock)
    {
        return new BalanceRecordDesk(
            store,
            BalanceRecordRules.DefaultBalanceRecordValidator(),
            clock,
            new List<string>());
    }
}
