namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global booking tiers by code.</summary>
public interface IGlobalBookingTierStore
{
    GlobalBookingTier? Find(string code);
    void Save(GlobalBookingTier item);
    bool Remove(string code);
    List<GlobalBookingTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalBookingTierStore(Dictionary<string, GlobalBookingTier> items, List<string> order) : IGlobalBookingTierStore
{
    public Dictionary<string, GlobalBookingTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalBookingTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalBookingTier item)
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

    public List<GlobalBookingTier> List()
    {
        var outValue = new List<GlobalBookingTier>();
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
public sealed class CachedGlobalBookingTierStore(MemoryGlobalBookingTierStore inner, GlobalBookingTierCache cache, int hits, int misses) : IGlobalBookingTierStore
{
    public MemoryGlobalBookingTierStore Inner { get; set; } = inner;
    public GlobalBookingTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalBookingTier? Find(string code)
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

    public void Save(GlobalBookingTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalBookingTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global booking tiers. Empty fields match all.</summary>
public sealed record GlobalBookingTierQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalBookingTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalBookingTier> Apply(List<GlobalBookingTier> items)
    {
        var matched = Collections.Filter(items, (GlobalBookingTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global booking tier store.</summary>
public static class GlobalBookingTierStore
{
    public static IGlobalBookingTierStore NewMemoryGlobalBookingTierStore()
    {
        return new MemoryGlobalBookingTierStore(new Dictionary<string, GlobalBookingTier>(), new List<string>());
    }

    public static IGlobalBookingTierStore NewCachedGlobalBookingTierStore(MemoryGlobalBookingTierStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalBookingTierStore(inner, GlobalBookingTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalBookingTier> FindGlobalBookingTiers(IGlobalBookingTierStore store, GlobalBookingTierQuery query)
    {
        return query.Apply(store.List());
    }
}
