namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered span groups by code.</summary>
public interface ITieredSpanGroupStore
{
    TieredSpanGroup? Find(string code);
    void Save(TieredSpanGroup item);
    bool Remove(string code);
    List<TieredSpanGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSpanGroupStore(Dictionary<string, TieredSpanGroup> items, List<string> order) : ITieredSpanGroupStore
{
    public Dictionary<string, TieredSpanGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSpanGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSpanGroup item)
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

    public List<TieredSpanGroup> List()
    {
        var outValue = new List<TieredSpanGroup>();
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
public sealed class CachedTieredSpanGroupStore(MemoryTieredSpanGroupStore inner, TieredSpanGroupCache cache, int hits, int misses) : ITieredSpanGroupStore
{
    public MemoryTieredSpanGroupStore Inner { get; set; } = inner;
    public TieredSpanGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSpanGroup? Find(string code)
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

    public void Save(TieredSpanGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSpanGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered span groups. Empty fields match all.</summary>
public sealed record TieredSpanGroupQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSpanGroup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSpanGroup> Apply(List<TieredSpanGroup> items)
    {
        var matched = Collections.Filter(items, (TieredSpanGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered span group store.</summary>
public static class TieredSpanGroupStore
{
    public static ITieredSpanGroupStore NewMemoryTieredSpanGroupStore()
    {
        return new MemoryTieredSpanGroupStore(new Dictionary<string, TieredSpanGroup>(), new List<string>());
    }

    public static ITieredSpanGroupStore NewCachedTieredSpanGroupStore(MemoryTieredSpanGroupStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSpanGroupStore(inner, TieredSpanGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredSpanGroup> FindTieredSpanGroups(ITieredSpanGroupStore store, TieredSpanGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
