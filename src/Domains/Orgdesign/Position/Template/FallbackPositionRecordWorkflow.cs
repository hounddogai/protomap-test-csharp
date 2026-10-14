namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback position records through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPositionRecordCoordinator(IFallbackPositionRecordStore Store, FallbackPositionRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback position record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPositionRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback position record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPositionRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of fallback position record workflow.</summary>
public static class FallbackPositionRecordWorkflow
{
    public static FallbackPositionRecordCoordinator NewFallbackPositionRecordCoordinator(IFallbackPositionRecordStore store, IClock clock)
    {
        return new FallbackPositionRecordCoordinator(
            store,
            FallbackPositionRecordRules.DefaultFallbackPositionRecordValidator(),
            clock,
            new List<string>());
    }
}
