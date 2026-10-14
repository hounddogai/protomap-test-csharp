namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves occupancy preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyPreferenceDesk(IOccupancyPreferenceStore Store, OccupancyPreferenceValidator Validator, IClock Clock, List<string> Journal, OccupancyPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyPreferenceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyPreferenceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy preference {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of occupancy preference workflow.</summary>
public static class OccupancyPreferenceWorkflow
{
    public static OccupancyPreferenceDesk NewOccupancyPreferenceDesk(IOccupancyPreferenceStore store, IClock clock, Registry flags)
    {
        return new OccupancyPreferenceDesk(
            store,
            OccupancyPreferenceRules.DefaultOccupancyPreferenceValidator(),
            clock,
            new List<string>(),
            OccupancyPreferenceEngine.DefaultOccupancyPreferenceEngine(flags));
    }
}
