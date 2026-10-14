namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool reservations by code.</summary>
public interface IAdaptiveBonusPoolReservationStore
{
    AdaptiveBonusPoolReservation? Find(string code);
    void Save(AdaptiveBonusPoolReservation item);
    bool Remove(string code);
    List<AdaptiveBonusPoolReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolReservationStore(Dictionary<string, AdaptiveBonusPoolReservation> items, List<string> order) : IAdaptiveBonusPoolReservationStore
{
    public Dictionary<string, AdaptiveBonusPoolReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolReservation item)
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

    public List<AdaptiveBonusPoolReservation> List()
    {
        var outValue = new List<AdaptiveBonusPoolReservation>();
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
public sealed class CachedAdaptiveBonusPoolReservationStore(MemoryAdaptiveBonusPoolReservationStore inner, AdaptiveBonusPoolReservationCache cache, int hits, int misses) : IAdaptiveBonusPoolReservationStore
{
    public MemoryAdaptiveBonusPoolReservationStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolReservation? Find(string code)
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

    public void Save(AdaptiveBonusPoolReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool reservations. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolReservationQuery(string Unit, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolReservation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBonusPoolReservation> Apply(List<AdaptiveBonusPoolReservation> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool reservation store.</summary>
public static class AdaptiveBonusPoolReservationStore
{
    public static IAdaptiveBonusPoolReservationStore NewMemoryAdaptiveBonusPoolReservationStore()
    {
        return new MemoryAdaptiveBonusPoolReservationStore(
            new Dictionary<string, AdaptiveBonusPoolReservation>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolReservationStore NewCachedAdaptiveBonusPoolReservationStore(MemoryAdaptiveBonusPoolReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolReservationStore(
            inner,
            AdaptiveBonusPoolReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolReservation> FindAdaptiveBonusPoolReservations(IAdaptiveBonusPoolReservationStore store, AdaptiveBonusPoolReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
