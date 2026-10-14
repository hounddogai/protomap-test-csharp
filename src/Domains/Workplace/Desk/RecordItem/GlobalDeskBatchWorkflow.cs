namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global desk batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalDeskBatchManager(IGlobalDeskBatchStore Store, GlobalDeskBatchValidator Validator, IClock Clock, List<string> Journal, GlobalDeskBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global desk batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalDeskBatchStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global desk batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalDeskBatchStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global desk batch {code} not found");
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

/// <summary>Functions of global desk batch workflow.</summary>
public static class GlobalDeskBatchWorkflow
{
    public static GlobalDeskBatchManager NewGlobalDeskBatchManager(IGlobalDeskBatchStore store, IClock clock, Registry flags)
    {
        return new GlobalDeskBatchManager(
            store,
            GlobalDeskBatchRules.DefaultGlobalDeskBatchValidator(),
            clock,
            new List<string>(),
            GlobalDeskBatchEngine.DefaultGlobalDeskBatchEngine(flags));
    }
}
