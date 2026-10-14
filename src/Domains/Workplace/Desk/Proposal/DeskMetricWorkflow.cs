namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskMetricCoordinator(IDeskMetricStore Store, DeskMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskMetricStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskMetricStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of desk metric workflow.</summary>
public static class DeskMetricWorkflow
{
    public static DeskMetricCoordinator NewDeskMetricCoordinator(IDeskMetricStore store, IClock clock)
    {
        return new DeskMetricCoordinator(
            store,
            DeskMetricRules.DefaultDeskMetricValidator(),
            clock,
            new List<string>());
    }
}
