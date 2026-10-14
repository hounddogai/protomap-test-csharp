namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountSnapshotService(IHeadcountSnapshotStore Store, HeadcountSnapshotValidator Validator, IClock Clock, List<string> Journal, HeadcountSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountSnapshotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountSnapshotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount snapshot {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of headcount snapshot workflow.</summary>
public static class HeadcountSnapshotWorkflow
{
    public static HeadcountSnapshotService NewHeadcountSnapshotService(IHeadcountSnapshotStore store, IClock clock)
    {
        return new HeadcountSnapshotService(
            store,
            HeadcountSnapshotRules.DefaultHeadcountSnapshotValidator(),
            clock,
            new List<string>(),
            HeadcountSnapshotEngine.DefaultHeadcountSnapshotEngine());
    }
}
