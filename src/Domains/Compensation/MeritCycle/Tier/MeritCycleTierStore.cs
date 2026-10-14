namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle tiers by code.</summary>
public interface IMeritCycleTierStore
{
    MeritCycleTier? Find(string code);
    void Save(MeritCycleTier item);
    bool Remove(string code);
    List<MeritCycleTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleTierStore(Dictionary<string, MeritCycleTier> items, List<string> order) : IMeritCycleTierStore
{
    public Dictionary<string, MeritCycleTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleTier item)
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

    public List<MeritCycleTier> List()
    {
        var outValue = new List<MeritCycleTier>();
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
public sealed class CachedMeritCycleTierStore(MemoryMeritCycleTierStore inner, MeritCycleTierCache cache, int hits, int misses) : IMeritCycleTierStore
{
    public MemoryMeritCycleTierStore Inner { get; set; } = inner;
    public MeritCycleTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleTier? Find(string code)
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

    public void Save(MeritCycleTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle tiers. Empty fields match all.</summary>
public sealed record MeritCycleTierQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleTier> Apply(List<MeritCycleTier> items)
    {
        var matched = Collections.Filter(items, (MeritCycleTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle tier store.</summary>
public static class MeritCycleTierStore
{
    public static IMeritCycleTierStore NewMemoryMeritCycleTierStore()
    {
        return new MemoryMeritCycleTierStore(new Dictionary<string, MeritCycleTier>(), new List<string>());
    }

    public static IMeritCycleTierStore NewCachedMeritCycleTierStore(MemoryMeritCycleTierStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleTierStore(inner, MeritCycleTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MeritCycleTier> FindMeritCycleTiers(IMeritCycleTierStore store, MeritCycleTierQuery query)
    {
        return query.Apply(store.List());
    }
}
