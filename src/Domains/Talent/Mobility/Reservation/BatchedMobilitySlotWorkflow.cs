namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched mobility slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedMobilitySlotWorkflow(IBatchedMobilitySlotStore Store, BatchedMobilitySlotValidator Validator, IClock Clock, List<string> Journal)
{
    public static BatchedMobilitySlotWorkflow NewBatchedMobilitySlotWorkflow(IBatchedMobilitySlotStore store, IClock clock)
    {
        return new BatchedMobilitySlotWorkflow(
            store,
            BatchedMobilitySlotRules.DefaultBatchedMobilitySlotValidator(),
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
            throw new InvalidOperationException($"batched mobility slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedMobilitySlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched mobility slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedMobilitySlotStatus.Closed);
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
            if (item.ExpiresAt > cutoff)
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
