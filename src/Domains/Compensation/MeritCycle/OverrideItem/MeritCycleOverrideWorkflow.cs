namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleOverrideService(IMeritCycleOverrideStore Store, MeritCycleOverrideValidator Validator, IClock Clock, List<string> Journal, MeritCycleOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle override {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of merit cycle override workflow.</summary>
public static class MeritCycleOverrideWorkflow
{
    public static MeritCycleOverrideService NewMeritCycleOverrideService(IMeritCycleOverrideStore store, IClock clock, Registry flags)
    {
        return new MeritCycleOverrideService(
            store,
            MeritCycleOverrideRules.DefaultMeritCycleOverrideValidator(),
            clock,
            new List<string>(),
            MeritCycleOverrideEngine.DefaultMeritCycleOverrideEngine(flags));
    }
}
