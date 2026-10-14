namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pool entries by code.</summary>
public interface ITieredPoolEntryStore
{
    TieredPoolEntry? Find(string code);
    void Save(TieredPoolEntry item);
    bool Remove(string code);
    List<TieredPoolEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPoolEntryStore(Dictionary<string, TieredPoolEntry> items, List<string> order) : ITieredPoolEntryStore
{
    public Dictionary<string, TieredPoolEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPoolEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPoolEntry item)
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

    public List<TieredPoolEntry> List()
    {
        var outValue = new List<TieredPoolEntry>();
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
public sealed class CachedTieredPoolEntryStore(MemoryTieredPoolEntryStore inner, TieredPoolEntryCache cache, int hits, int misses) : ITieredPoolEntryStore
{
    public MemoryTieredPoolEntryStore Inner { get; set; } = inner;
    public TieredPoolEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPoolEntry? Find(string code)
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

    public void Save(TieredPoolEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPoolEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pool entries. Empty fields match all.</summary>
public sealed record TieredPoolEntryQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPoolEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPoolEntry> Apply(List<TieredPoolEntry> items)
    {
        var matched = Collections.Filter(items, (TieredPoolEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pool entry store.</summary>
public static class TieredPoolEntryStore
{
    public static ITieredPoolEntryStore NewMemoryTieredPoolEntryStore()
    {
        return new MemoryTieredPoolEntryStore(new Dictionary<string, TieredPoolEntry>(), new List<string>());
    }

    public static ITieredPoolEntryStore NewCachedTieredPoolEntryStore(MemoryTieredPoolEntryStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPoolEntryStore(inner, TieredPoolEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TieredPoolEntry> FindTieredPoolEntries(ITieredPoolEntryStore store, TieredPoolEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
