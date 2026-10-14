namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleEntryWorkflow(IMeritCycleEntryStore Store, MeritCycleEntryValidator Validator, IClock Clock, List<string> Journal, MeritCycleEntryEngine Engine)
{
    public static MeritCycleEntryWorkflow NewMeritCycleEntryWorkflow(IMeritCycleEntryStore store, IClock clock, Registry flags)
    {
        return new MeritCycleEntryWorkflow(
            store,
            MeritCycleEntryRules.DefaultMeritCycleEntryValidator(),
            clock,
            new List<string>(),
            MeritCycleEntryEngine.DefaultMeritCycleEntryEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle entry {code} not found");
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
