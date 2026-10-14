namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient merit cycle windows through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientMeritCycleWindowManager(ILenientMeritCycleWindowStore Store, LenientMeritCycleWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient merit cycle window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientMeritCycleWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient merit cycle window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientMeritCycleWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of lenient merit cycle window workflow.</summary>
public static class LenientMeritCycleWindowWorkflow
{
    public static LenientMeritCycleWindowManager NewLenientMeritCycleWindowManager(ILenientMeritCycleWindowStore store, IClock clock)
    {
        return new LenientMeritCycleWindowManager(
            store,
            LenientMeritCycleWindowRules.DefaultLenientMeritCycleWindowValidator(),
            clock,
            new List<string>());
    }
}
