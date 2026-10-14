namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleSummaryService(IMeritCycleSummaryStore Store, MeritCycleSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of merit cycle summary workflow.</summary>
public static class MeritCycleSummaryWorkflow
{
    public static MeritCycleSummaryService NewMeritCycleSummaryService(IMeritCycleSummaryStore store, IClock clock)
    {
        return new MeritCycleSummaryService(
            store,
            MeritCycleSummaryRules.DefaultMeritCycleSummaryValidator(),
            clock,
            new List<string>());
    }
}
