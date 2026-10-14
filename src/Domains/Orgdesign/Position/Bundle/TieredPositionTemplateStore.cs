namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered position templates by code.</summary>
public interface ITieredPositionTemplateStore
{
    TieredPositionTemplate? Find(string code);
    void Save(TieredPositionTemplate item);
    bool Remove(string code);
    List<TieredPositionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPositionTemplateStore(Dictionary<string, TieredPositionTemplate> items, List<string> order) : ITieredPositionTemplateStore
{
    public Dictionary<string, TieredPositionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPositionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPositionTemplate item)
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

    public List<TieredPositionTemplate> List()
    {
        var outValue = new List<TieredPositionTemplate>();
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
public sealed class CachedTieredPositionTemplateStore(MemoryTieredPositionTemplateStore inner, TieredPositionTemplateCache cache, int hits, int misses) : ITieredPositionTemplateStore
{
    public MemoryTieredPositionTemplateStore Inner { get; set; } = inner;
    public TieredPositionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPositionTemplate? Find(string code)
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

    public void Save(TieredPositionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPositionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered position templates. Empty fields match all.</summary>
public sealed record TieredPositionTemplateQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPositionTemplate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<TieredPositionTemplate> Apply(List<TieredPositionTemplate> items)
    {
        var matched = Collections.Filter(items, (TieredPositionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered position template store.</summary>
public static class TieredPositionTemplateStore
{
    public static ITieredPositionTemplateStore NewMemoryTieredPositionTemplateStore()
    {
        return new MemoryTieredPositionTemplateStore(
            new Dictionary<string, TieredPositionTemplate>(),
            new List<string>());
    }

    public static ITieredPositionTemplateStore NewCachedTieredPositionTemplateStore(MemoryTieredPositionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPositionTemplateStore(
            inner,
            TieredPositionTemplateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredPositionTemplate> FindTieredPositionTemplates(ITieredPositionTemplateStore store, TieredPositionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
