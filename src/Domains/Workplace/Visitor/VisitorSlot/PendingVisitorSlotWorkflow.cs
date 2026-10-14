namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending visitor slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingVisitorSlotDesk(IPendingVisitorSlotStore Store, PendingVisitorSlotValidator Validator, IClock Clock, List<string> Journal, PendingVisitorSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingVisitorSlotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingVisitorSlotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor slot {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pending visitor slot workflow.</summary>
public static class PendingVisitorSlotWorkflow
{
    public static PendingVisitorSlotDesk NewPendingVisitorSlotDesk(IPendingVisitorSlotStore store, IClock clock, Registry flags)
    {
        return new PendingVisitorSlotDesk(
            store,
            PendingVisitorSlotRules.DefaultPendingVisitorSlotValidator(),
            clock,
            new List<string>(),
            PendingVisitorSlotEngine.DefaultPendingVisitorSlotEngine(flags));
    }
}
