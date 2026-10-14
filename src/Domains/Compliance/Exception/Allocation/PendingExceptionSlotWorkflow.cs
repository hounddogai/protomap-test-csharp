namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending exception slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingExceptionSlotService(IPendingExceptionSlotStore Store, PendingExceptionSlotValidator Validator, IClock Clock, List<string> Journal, PendingExceptionSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingExceptionSlotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingExceptionSlotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending exception slot {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pending exception slot workflow.</summary>
public static class PendingExceptionSlotWorkflow
{
    public static PendingExceptionSlotService NewPendingExceptionSlotService(IPendingExceptionSlotStore store, IClock clock, Registry flags)
    {
        return new PendingExceptionSlotService(
            store,
            PendingExceptionSlotRules.DefaultPendingExceptionSlotValidator(),
            clock,
            new List<string>(),
            PendingExceptionSlotEngine.DefaultPendingExceptionSlotEngine(flags));
    }
}
