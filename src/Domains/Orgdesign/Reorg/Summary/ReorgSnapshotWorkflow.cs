namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgSnapshotWorkflow(IReorgSnapshotStore Store, ReorgSnapshotValidator Validator, IClock Clock, List<string> Journal, ReorgSnapshotEngine Engine)
{
    public static ReorgSnapshotWorkflow NewReorgSnapshotWorkflow(IReorgSnapshotStore store, IClock clock)
    {
        return new ReorgSnapshotWorkflow(
            store,
            ReorgSnapshotRules.DefaultReorgSnapshotValidator(),
            clock,
            new List<string>(),
            ReorgSnapshotEngine.DefaultReorgSnapshotEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg snapshot {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
