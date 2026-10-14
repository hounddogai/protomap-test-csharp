namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped scenario reservations by code.</summary>
public interface ICappedScenarioReservationStore
{
    CappedScenarioReservation? Find(string code);
    void Save(CappedScenarioReservation item);
    bool Remove(string code);
    List<CappedScenarioReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedScenarioReservationStore(Dictionary<string, CappedScenarioReservation> items, List<string> order) : ICappedScenarioReservationStore
{
    public Dictionary<string, CappedScenarioReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedScenarioReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedScenarioReservation item)
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

    public List<CappedScenarioReservation> List()
    {
        var outValue = new List<CappedScenarioReservation>();
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
public sealed class CachedCappedScenarioReservationStore(MemoryCappedScenarioReservationStore inner, CappedScenarioReservationCache cache, int hits, int misses) : ICappedScenarioReservationStore
{
    public MemoryCappedScenarioReservationStore Inner { get; set; } = inner;
    public CappedScenarioReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedScenarioReservation? Find(string code)
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

    public void Save(CappedScenarioReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedScenarioReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped scenario reservations. Empty fields match all.</summary>
public sealed record CappedScenarioReservationQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(CappedScenarioReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedScenarioReservation> Apply(List<CappedScenarioReservation> items)
    {
        var matched = Collections.Filter(items, (CappedScenarioReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped scenario reservation store.</summary>
public static class CappedScenarioReservationStore
{
    public static ICappedScenarioReservationStore NewMemoryCappedScenarioReservationStore()
    {
        return new MemoryCappedScenarioReservationStore(
            new Dictionary<string, CappedScenarioReservation>(),
            new List<string>());
    }

    public static ICappedScenarioReservationStore NewCachedCappedScenarioReservationStore(MemoryCappedScenarioReservationStore inner, int ttl, IClock clock)
    {
        return new CachedCappedScenarioReservationStore(
            inner,
            CappedScenarioReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedScenarioReservation> FindCappedScenarioReservations(ICappedScenarioReservationStore store, CappedScenarioReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
