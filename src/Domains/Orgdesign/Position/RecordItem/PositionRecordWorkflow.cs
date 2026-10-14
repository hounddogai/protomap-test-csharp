namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position records through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionRecordManager(IPositionRecordStore Store, PositionRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionRecordStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of position record workflow.</summary>
public static class PositionRecordWorkflow
{
    public static PositionRecordManager NewPositionRecordManager(IPositionRecordStore store, IClock clock)
    {
        return new PositionRecordManager(
            store,
            PositionRecordRules.DefaultPositionRecordValidator(),
            clock,
            new List<string>());
    }
}
