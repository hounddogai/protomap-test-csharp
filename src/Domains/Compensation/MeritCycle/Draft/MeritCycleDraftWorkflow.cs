namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleDraftCoordinator(IMeritCycleDraftStore Store, MeritCycleDraftValidator Validator, IClock Clock, List<string> Journal, MeritCycleDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleDraftStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleDraftStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle draft {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of merit cycle draft workflow.</summary>
public static class MeritCycleDraftWorkflow
{
    public static MeritCycleDraftCoordinator NewMeritCycleDraftCoordinator(IMeritCycleDraftStore store, IClock clock)
    {
        return new MeritCycleDraftCoordinator(
            store,
            MeritCycleDraftRules.DefaultMeritCycleDraftValidator(),
            clock,
            new List<string>(),
            MeritCycleDraftEngine.DefaultMeritCycleDraftEngine());
    }
}
