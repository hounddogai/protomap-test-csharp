namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle mappings by code.</summary>
public interface IMeritCycleMappingStore
{
    MeritCycleMapping? Find(string code);
    void Save(MeritCycleMapping item);
    bool Remove(string code);
    List<MeritCycleMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleMappingStore(Dictionary<string, MeritCycleMapping> items, List<string> order) : IMeritCycleMappingStore
{
    public Dictionary<string, MeritCycleMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleMapping item)
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

    public List<MeritCycleMapping> List()
    {
        var outValue = new List<MeritCycleMapping>();
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
public sealed class CachedMeritCycleMappingStore(MemoryMeritCycleMappingStore inner, MeritCycleMappingCache cache, int hits, int misses) : IMeritCycleMappingStore
{
    public MemoryMeritCycleMappingStore Inner { get; set; } = inner;
    public MeritCycleMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleMapping? Find(string code)
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

    public void Save(MeritCycleMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle mappings. Empty fields match all.</summary>
public sealed record MeritCycleMappingQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleMapping item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleMapping> Apply(List<MeritCycleMapping> items)
    {
        var matched = Collections.Filter(items, (MeritCycleMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle mapping store.</summary>
public static class MeritCycleMappingStore
{
    public static IMeritCycleMappingStore NewMemoryMeritCycleMappingStore()
    {
        return new MemoryMeritCycleMappingStore(new Dictionary<string, MeritCycleMapping>(), new List<string>());
    }

    public static IMeritCycleMappingStore NewCachedMeritCycleMappingStore(MemoryMeritCycleMappingStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleMappingStore(inner, MeritCycleMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MeritCycleMapping> FindMeritCycleMappings(IMeritCycleMappingStore store, MeritCycleMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
