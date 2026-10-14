namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy booking rollups by code.</summary>
public interface ILegacyBookingRollupStore
{
    LegacyBookingRollup? Find(string code);
    void Save(LegacyBookingRollup item);
    bool Remove(string code);
    List<LegacyBookingRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyBookingRollupStore(Dictionary<string, LegacyBookingRollup> items, List<string> order) : ILegacyBookingRollupStore
{
    public Dictionary<string, LegacyBookingRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyBookingRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyBookingRollup item)
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

    public List<LegacyBookingRollup> List()
    {
        var outValue = new List<LegacyBookingRollup>();
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
public sealed class CachedLegacyBookingRollupStore(MemoryLegacyBookingRollupStore inner, LegacyBookingRollupCache cache, int hits, int misses) : ILegacyBookingRollupStore
{
    public MemoryLegacyBookingRollupStore Inner { get; set; } = inner;
    public LegacyBookingRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyBookingRollup? Find(string code)
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

    public void Save(LegacyBookingRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyBookingRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy booking rollups. Empty fields match all.</summary>
public sealed record LegacyBookingRollupQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyBookingRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyBookingRollup> Apply(List<LegacyBookingRollup> items)
    {
        var matched = Collections.Filter(items, (LegacyBookingRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy booking rollup store.</summary>
public static class LegacyBookingRollupStore
{
    public static ILegacyBookingRollupStore NewMemoryLegacyBookingRollupStore()
    {
        return new MemoryLegacyBookingRollupStore(new Dictionary<string, LegacyBookingRollup>(), new List<string>());
    }

    public static ILegacyBookingRollupStore NewCachedLegacyBookingRollupStore(MemoryLegacyBookingRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyBookingRollupStore(inner, LegacyBookingRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyBookingRollup> FindLegacyBookingRollups(ILegacyBookingRollupStore store, LegacyBookingRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
