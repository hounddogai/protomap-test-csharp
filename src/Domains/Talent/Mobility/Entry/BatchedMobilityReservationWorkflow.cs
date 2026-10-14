namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched mobility reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedMobilityReservationService(IBatchedMobilityReservationStore Store, BatchedMobilityReservationValidator Validator, IClock Clock, List<string> Journal, BatchedMobilityReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedMobilityReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedMobilityReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility reservation {code} not found");
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

/// <summary>Functions of batched mobility reservation workflow.</summary>
public static class BatchedMobilityReservationWorkflow
{
    public static BatchedMobilityReservationService NewBatchedMobilityReservationService(IBatchedMobilityReservationStore store, IClock clock, Registry flags)
    {
        return new BatchedMobilityReservationService(
            store,
            BatchedMobilityReservationRules.DefaultBatchedMobilityReservationValidator(),
            clock,
            new List<string>(),
            BatchedMobilityReservationEngine.DefaultBatchedMobilityReservationEngine(flags));
    }
}
