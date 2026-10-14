namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pool groups by code.</summary>
public interface ITieredPoolGroupStore
{
    TieredPoolGroup? Find(string code);
    void Save(TieredPoolGroup item);
    bool Remove(string code);
    List<TieredPoolGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPoolGroupStore(Dictionary<string, TieredPoolGroup> items, List<string> order) : ITieredPoolGroupStore
{
    public Dictionary<string, TieredPoolGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPoolGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPoolGroup item)
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

    public List<TieredPoolGroup> List()
    {
        var outValue = new List<TieredPoolGroup>();
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
public sealed class CachedTieredPoolGroupStore(MemoryTieredPoolGroupStore inner, TieredPoolGroupCache cache, int hits, int misses) : ITieredPoolGroupStore
{
    public MemoryTieredPoolGroupStore Inner { get; set; } = inner;
    public TieredPoolGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPoolGroup? Find(string code)
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

    public void Save(TieredPoolGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPoolGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pool groups. Empty fields match all.</summary>
public sealed record TieredPoolGroupQuery(string Department, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPoolGroup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPoolGroup> Apply(List<TieredPoolGroup> items)
    {
        var matched = Collections.Filter(items, (TieredPoolGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pool group store.</summary>
public static class TieredPoolGroupStore
{
    public static ITieredPoolGroupStore NewMemoryTieredPoolGroupStore()
    {
        return new MemoryTieredPoolGroupStore(new Dictionary<string, TieredPoolGroup>(), new List<string>());
    }

    public static ITieredPoolGroupStore NewCachedTieredPoolGroupStore(MemoryTieredPoolGroupStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPoolGroupStore(inner, TieredPoolGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TieredPoolGroup> FindTieredPoolGroups(ITieredPoolGroupStore store, TieredPoolGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
