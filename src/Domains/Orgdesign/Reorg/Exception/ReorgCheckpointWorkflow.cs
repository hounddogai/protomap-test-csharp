namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgCheckpointDesk(IReorgCheckpointStore Store, ReorgCheckpointValidator Validator, IClock Clock, List<string> Journal, ReorgCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgCheckpointStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgCheckpointStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg checkpoint {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of reorg checkpoint workflow.</summary>
public static class ReorgCheckpointWorkflow
{
    public static ReorgCheckpointDesk NewReorgCheckpointDesk(IReorgCheckpointStore store, IClock clock)
    {
        return new ReorgCheckpointDesk(
            store,
            ReorgCheckpointRules.DefaultReorgCheckpointValidator(),
            clock,
            new List<string>(),
            ReorgCheckpointEngine.DefaultReorgCheckpointEngine());
    }
}
