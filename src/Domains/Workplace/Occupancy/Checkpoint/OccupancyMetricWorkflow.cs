namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyMetricWorkflow(IOccupancyMetricStore Store, OccupancyMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static OccupancyMetricWorkflow NewOccupancyMetricWorkflow(IOccupancyMetricStore store, IClock clock)
    {
        return new OccupancyMetricWorkflow(
            store,
            OccupancyMetricRules.DefaultOccupancyMetricValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyMetricStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyMetricStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
