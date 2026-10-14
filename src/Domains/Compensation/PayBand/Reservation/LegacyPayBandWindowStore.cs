namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay band windows by code.</summary>
public interface ILegacyPayBandWindowStore
{
    LegacyPayBandWindow? Find(string code);
    void Save(LegacyPayBandWindow item);
    bool Remove(string code);
    List<LegacyPayBandWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayBandWindowStore(Dictionary<string, LegacyPayBandWindow> items, List<string> order) : ILegacyPayBandWindowStore
{
    public Dictionary<string, LegacyPayBandWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayBandWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayBandWindow item)
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

    public List<LegacyPayBandWindow> List()
    {
        var outValue = new List<LegacyPayBandWindow>();
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
public sealed class CachedLegacyPayBandWindowStore(MemoryLegacyPayBandWindowStore inner, LegacyPayBandWindowCache cache, int hits, int misses) : ILegacyPayBandWindowStore
{
    public MemoryLegacyPayBandWindowStore Inner { get; set; } = inner;
    public LegacyPayBandWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayBandWindow? Find(string code)
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

    public void Save(LegacyPayBandWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayBandWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay band windows. Empty fields match all.</summary>
public sealed record LegacyPayBandWindowQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayBandWindow item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPayBandWindow> Apply(List<LegacyPayBandWindow> items)
    {
        var matched = Collections.Filter(items, (LegacyPayBandWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay band window store.</summary>
public static class LegacyPayBandWindowStore
{
    public static ILegacyPayBandWindowStore NewMemoryLegacyPayBandWindowStore()
    {
        return new MemoryLegacyPayBandWindowStore(new Dictionary<string, LegacyPayBandWindow>(), new List<string>());
    }

    public static ILegacyPayBandWindowStore NewCachedLegacyPayBandWindowStore(MemoryLegacyPayBandWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayBandWindowStore(inner, LegacyPayBandWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacyPayBandWindow> FindLegacyPayBandWindows(ILegacyPayBandWindowStore store, LegacyPayBandWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
