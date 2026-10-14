namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgSlotManager(IReorgSlotStore Store, ReorgSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of reorg slot workflow.</summary>
public static class ReorgSlotWorkflow
{
    public static ReorgSlotManager NewReorgSlotManager(IReorgSlotStore store, IClock clock)
    {
        return new ReorgSlotManager(store, ReorgSlotRules.DefaultReorgSlotValidator(), clock, new List<string>());
    }
}
