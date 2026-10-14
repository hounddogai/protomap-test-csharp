namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position mappings by code.</summary>
public interface IPositionMappingStore
{
    PositionMapping? Find(string code);
    void Save(PositionMapping item);
    bool Remove(string code);
    List<PositionMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionMappingStore(Dictionary<string, PositionMapping> items, List<string> order) : IPositionMappingStore
{
    public Dictionary<string, PositionMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionMapping item)
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

    public List<PositionMapping> List()
    {
        var outValue = new List<PositionMapping>();
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
public sealed class CachedPositionMappingStore(MemoryPositionMappingStore inner, PositionMappingCache cache, int hits, int misses) : IPositionMappingStore
{
    public MemoryPositionMappingStore Inner { get; set; } = inner;
    public PositionMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionMapping? Find(string code)
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

    public void Save(PositionMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position mappings. Empty fields match all.</summary>
public sealed record PositionMappingQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PositionMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PositionMapping> Apply(List<PositionMapping> items)
    {
        var matched = Collections.Filter(items, (PositionMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position mapping store.</summary>
public static class PositionMappingStore
{
    public static IPositionMappingStore NewMemoryPositionMappingStore()
    {
        return new MemoryPositionMappingStore(new Dictionary<string, PositionMapping>(), new List<string>());
    }

    public static IPositionMappingStore NewCachedPositionMappingStore(MemoryPositionMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPositionMappingStore(inner, PositionMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionMapping> FindPositionMappings(IPositionMappingStore store, PositionMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
