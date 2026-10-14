namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling balance records through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingBalanceRecordService(IRollingBalanceRecordStore Store, RollingBalanceRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling balance record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingBalanceRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling balance record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingBalanceRecordStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of rolling balance record workflow.</summary>
public static class RollingBalanceRecordWorkflow
{
    public static RollingBalanceRecordService NewRollingBalanceRecordService(IRollingBalanceRecordStore store, IClock clock)
    {
        return new RollingBalanceRecordService(
            store,
            RollingBalanceRecordRules.DefaultRollingBalanceRecordValidator(),
            clock,
            new List<string>());
    }
}
