namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool templates by code.</summary>
public interface IPoolTemplateStore
{
    PoolTemplate? Find(string code);
    void Save(PoolTemplate item);
    bool Remove(string code);
    List<PoolTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolTemplateStore(Dictionary<string, PoolTemplate> items, List<string> order) : IPoolTemplateStore
{
    public Dictionary<string, PoolTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolTemplate item)
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

    public List<PoolTemplate> List()
    {
        var outValue = new List<PoolTemplate>();
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
public sealed class CachedPoolTemplateStore(MemoryPoolTemplateStore inner, PoolTemplateCache cache, int hits, int misses) : IPoolTemplateStore
{
    public MemoryPoolTemplateStore Inner { get; set; } = inner;
    public PoolTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolTemplate? Find(string code)
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

    public void Save(PoolTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool templates. Empty fields match all.</summary>
public sealed record PoolTemplateQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PoolTemplate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<PoolTemplate> Apply(List<PoolTemplate> items)
    {
        var matched = Collections.Filter(items, (PoolTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool template store.</summary>
public static class PoolTemplateStore
{
    public static IPoolTemplateStore NewMemoryPoolTemplateStore()
    {
        return new MemoryPoolTemplateStore(new Dictionary<string, PoolTemplate>(), new List<string>());
    }

    public static IPoolTemplateStore NewCachedPoolTemplateStore(MemoryPoolTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPoolTemplateStore(inner, PoolTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolTemplate> FindPoolTemplates(IPoolTemplateStore store, PoolTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
