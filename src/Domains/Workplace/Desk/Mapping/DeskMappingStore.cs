namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk mappings by code.</summary>
public interface IDeskMappingStore
{
    DeskMapping? Find(string code);
    void Save(DeskMapping item);
    bool Remove(string code);
    List<DeskMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskMappingStore(Dictionary<string, DeskMapping> items, List<string> order) : IDeskMappingStore
{
    public Dictionary<string, DeskMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskMapping item)
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

    public List<DeskMapping> List()
    {
        var outValue = new List<DeskMapping>();
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
public sealed class CachedDeskMappingStore(MemoryDeskMappingStore inner, DeskMappingCache cache, int hits, int misses) : IDeskMappingStore
{
    public MemoryDeskMappingStore Inner { get; set; } = inner;
    public DeskMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskMapping? Find(string code)
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

    public void Save(DeskMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk mappings. Empty fields match all.</summary>
public sealed record DeskMappingQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(DeskMapping item)
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

    public List<DeskMapping> Apply(List<DeskMapping> items)
    {
        var matched = Collections.Filter(items, (DeskMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk mapping store.</summary>
public static class DeskMappingStore
{
    public static IDeskMappingStore NewMemoryDeskMappingStore()
    {
        return new MemoryDeskMappingStore(new Dictionary<string, DeskMapping>(), new List<string>());
    }

    public static IDeskMappingStore NewCachedDeskMappingStore(MemoryDeskMappingStore inner, int ttl, IClock clock)
    {
        return new CachedDeskMappingStore(inner, DeskMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeskMapping> FindDeskMappings(IDeskMappingStore store, DeskMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
