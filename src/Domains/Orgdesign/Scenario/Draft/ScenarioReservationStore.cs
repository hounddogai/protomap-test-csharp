namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario reservations by code.</summary>
public interface IScenarioReservationStore
{
    ScenarioReservation? Find(string code);
    void Save(ScenarioReservation item);
    bool Remove(string code);
    List<ScenarioReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioReservationStore(Dictionary<string, ScenarioReservation> items, List<string> order) : IScenarioReservationStore
{
    public Dictionary<string, ScenarioReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioReservation item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<ScenarioReservation> List()
    {
        var outValue = new List<ScenarioReservation>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedScenarioReservationStore(MemoryScenarioReservationStore inner, ScenarioReservationCache cache, int hits, int misses) : IScenarioReservationStore
{
    public MemoryScenarioReservationStore Inner { get; set; } = inner;
    public ScenarioReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioReservation? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(ScenarioReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario reservations. Empty fields match all.</summary>
public sealed record ScenarioReservationQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioReservation> Apply(List<ScenarioReservation> items)
    {
        var matched = Collections.Filter(items, (ScenarioReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario reservation store.</summary>
public static class ScenarioReservationStore
{
    public static IScenarioReservationStore NewMemoryScenarioReservationStore()
    {
        return new MemoryScenarioReservationStore(new Dictionary<string, ScenarioReservation>(), new List<string>());
    }

    public static IScenarioReservationStore NewCachedScenarioReservationStore(MemoryScenarioReservationStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioReservationStore(inner, ScenarioReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScenarioReservation> FindScenarioReservations(IScenarioReservationStore store, ScenarioReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
