namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount reservations by code.</summary>
public interface IHeadcountReservationStore
{
    HeadcountReservation? Find(string code);
    void Save(HeadcountReservation item);
    bool Remove(string code);
    List<HeadcountReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountReservationStore(Dictionary<string, HeadcountReservation> items, List<string> order) : IHeadcountReservationStore
{
    public Dictionary<string, HeadcountReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountReservation item)
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

    public List<HeadcountReservation> List()
    {
        var outValue = new List<HeadcountReservation>();
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
public sealed class CachedHeadcountReservationStore(MemoryHeadcountReservationStore inner, HeadcountReservationCache cache, int hits, int misses) : IHeadcountReservationStore
{
    public MemoryHeadcountReservationStore Inner { get; set; } = inner;
    public HeadcountReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountReservation? Find(string code)
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

    public void Save(HeadcountReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount reservations. Empty fields match all.</summary>
public sealed record HeadcountReservationQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountReservation> Apply(List<HeadcountReservation> items)
    {
        var matched = Collections.Filter(items, (HeadcountReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount reservation store.</summary>
public static class HeadcountReservationStore
{
    public static IHeadcountReservationStore NewMemoryHeadcountReservationStore()
    {
        return new MemoryHeadcountReservationStore(
            new Dictionary<string, HeadcountReservation>(),
            new List<string>());
    }

    public static IHeadcountReservationStore NewCachedHeadcountReservationStore(MemoryHeadcountReservationStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountReservationStore(inner, HeadcountReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountReservation> FindHeadcountReservations(IHeadcountReservationStore store, HeadcountReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
