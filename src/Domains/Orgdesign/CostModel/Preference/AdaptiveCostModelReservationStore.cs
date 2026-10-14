namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive cost model reservations by code.</summary>
public interface IAdaptiveCostModelReservationStore
{
    AdaptiveCostModelReservation? Find(string code);
    void Save(AdaptiveCostModelReservation item);
    bool Remove(string code);
    List<AdaptiveCostModelReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCostModelReservationStore(Dictionary<string, AdaptiveCostModelReservation> items, List<string> order) : IAdaptiveCostModelReservationStore
{
    public Dictionary<string, AdaptiveCostModelReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCostModelReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCostModelReservation item)
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

    public List<AdaptiveCostModelReservation> List()
    {
        var outValue = new List<AdaptiveCostModelReservation>();
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
public sealed class CachedAdaptiveCostModelReservationStore(MemoryAdaptiveCostModelReservationStore inner, AdaptiveCostModelReservationCache cache, int hits, int misses) : IAdaptiveCostModelReservationStore
{
    public MemoryAdaptiveCostModelReservationStore Inner { get; set; } = inner;
    public AdaptiveCostModelReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCostModelReservation? Find(string code)
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

    public void Save(AdaptiveCostModelReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCostModelReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive cost model reservations. Empty fields match all.</summary>
public sealed record AdaptiveCostModelReservationQuery(string Team, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCostModelReservation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCostModelReservation> Apply(List<AdaptiveCostModelReservation> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCostModelReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive cost model reservation store.</summary>
public static class AdaptiveCostModelReservationStore
{
    public static IAdaptiveCostModelReservationStore NewMemoryAdaptiveCostModelReservationStore()
    {
        return new MemoryAdaptiveCostModelReservationStore(
            new Dictionary<string, AdaptiveCostModelReservation>(),
            new List<string>());
    }

    public static IAdaptiveCostModelReservationStore NewCachedAdaptiveCostModelReservationStore(MemoryAdaptiveCostModelReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCostModelReservationStore(
            inner,
            AdaptiveCostModelReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCostModelReservation> FindAdaptiveCostModelReservations(IAdaptiveCostModelReservationStore store, AdaptiveCostModelReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
