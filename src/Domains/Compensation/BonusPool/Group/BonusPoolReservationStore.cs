namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool reservations by code.</summary>
public interface IBonusPoolReservationStore
{
    BonusPoolReservation? Find(string code);
    void Save(BonusPoolReservation item);
    bool Remove(string code);
    List<BonusPoolReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolReservationStore(Dictionary<string, BonusPoolReservation> items, List<string> order) : IBonusPoolReservationStore
{
    public Dictionary<string, BonusPoolReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolReservation item)
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

    public List<BonusPoolReservation> List()
    {
        var outValue = new List<BonusPoolReservation>();
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
public sealed class CachedBonusPoolReservationStore(MemoryBonusPoolReservationStore inner, BonusPoolReservationCache cache, int hits, int misses) : IBonusPoolReservationStore
{
    public MemoryBonusPoolReservationStore Inner { get; set; } = inner;
    public BonusPoolReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolReservation? Find(string code)
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

    public void Save(BonusPoolReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool reservations. Empty fields match all.</summary>
public sealed record BonusPoolReservationQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolReservation> Apply(List<BonusPoolReservation> items)
    {
        var matched = Collections.Filter(items, (BonusPoolReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool reservation store.</summary>
public static class BonusPoolReservationStore
{
    public static IBonusPoolReservationStore NewMemoryBonusPoolReservationStore()
    {
        return new MemoryBonusPoolReservationStore(
            new Dictionary<string, BonusPoolReservation>(),
            new List<string>());
    }

    public static IBonusPoolReservationStore NewCachedBonusPoolReservationStore(MemoryBonusPoolReservationStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolReservationStore(inner, BonusPoolReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BonusPoolReservation> FindBonusPoolReservations(IBonusPoolReservationStore store, BonusPoolReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
