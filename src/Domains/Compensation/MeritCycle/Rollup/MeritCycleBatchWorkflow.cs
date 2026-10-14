namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleBatchManager(IMeritCycleBatchStore Store, MeritCycleBatchValidator Validator, IClock Clock, List<string> Journal, MeritCycleBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleBatchStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleBatchStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle batch {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of merit cycle batch workflow.</summary>
public static class MeritCycleBatchWorkflow
{
    public static MeritCycleBatchManager NewMeritCycleBatchManager(IMeritCycleBatchStore store, IClock clock, Registry flags)
    {
        return new MeritCycleBatchManager(
            store,
            MeritCycleBatchRules.DefaultMeritCycleBatchValidator(),
            clock,
            new List<string>(),
            MeritCycleBatchEngine.DefaultMeritCycleBatchEngine(flags));
    }
}
