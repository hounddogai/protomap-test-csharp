namespace Acme.Workforce.Engagement.TownHall.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall reservations by code.</summary>
public interface ITownHallReservationStore
{
    TownHallReservation? Find(string code);
    void Save(TownHallReservation item);
    bool Remove(string code);
    List<TownHallReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallReservationStore(Dictionary<string, TownHallReservation> items, List<string> order) : ITownHallReservationStore
{
    public Dictionary<string, TownHallReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallReservation item)
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

    public List<TownHallReservation> List()
    {
        var outValue = new List<TownHallReservation>();
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
public sealed class CachedTownHallReservationStore(MemoryTownHallReservationStore inner, TownHallReservationCache cache, int hits, int misses) : ITownHallReservationStore
{
    public MemoryTownHallReservationStore Inner { get; set; } = inner;
    public TownHallReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallReservation? Find(string code)
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

    public void Save(TownHallReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall reservations. Empty fields match all.</summary>
public sealed record TownHallReservationQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallReservation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallReservation> Apply(List<TownHallReservation> items)
    {
        var matched = Collections.Filter(items, (TownHallReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall reservation store.</summary>
public static class TownHallReservationStore
{
    public static ITownHallReservationStore NewMemoryTownHallReservationStore()
    {
        return new MemoryTownHallReservationStore(new Dictionary<string, TownHallReservation>(), new List<string>());
    }

    public static ITownHallReservationStore NewCachedTownHallReservationStore(MemoryTownHallReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallReservationStore(inner, TownHallReservationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallReservation> FindTownHallReservations(ITownHallReservationStore store, TownHallReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
