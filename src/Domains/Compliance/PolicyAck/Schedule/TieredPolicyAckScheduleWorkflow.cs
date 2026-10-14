namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered policy ack schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredPolicyAckScheduleCoordinator(ITieredPolicyAckScheduleStore Store, TieredPolicyAckScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy ack schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPolicyAckScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy ack schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPolicyAckScheduleStatus.Withdrawn);
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

/// <summary>Functions of tiered policy ack schedule workflow.</summary>
public static class TieredPolicyAckScheduleWorkflow
{
    public static TieredPolicyAckScheduleCoordinator NewTieredPolicyAckScheduleCoordinator(ITieredPolicyAckScheduleStore store, IClock clock)
    {
        return new TieredPolicyAckScheduleCoordinator(
            store,
            TieredPolicyAckScheduleRules.DefaultTieredPolicyAckScheduleValidator(),
            clock,
            new List<string>());
    }
}
