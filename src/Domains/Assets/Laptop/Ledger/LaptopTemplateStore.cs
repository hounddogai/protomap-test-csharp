namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop templates by code.</summary>
public interface ILaptopTemplateStore
{
    LaptopTemplate? Find(string code);
    void Save(LaptopTemplate item);
    bool Remove(string code);
    List<LaptopTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopTemplateStore(Dictionary<string, LaptopTemplate> items, List<string> order) : ILaptopTemplateStore
{
    public Dictionary<string, LaptopTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopTemplate item)
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

    public List<LaptopTemplate> List()
    {
        var outValue = new List<LaptopTemplate>();
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
public sealed class CachedLaptopTemplateStore(MemoryLaptopTemplateStore inner, LaptopTemplateCache cache, int hits, int misses) : ILaptopTemplateStore
{
    public MemoryLaptopTemplateStore Inner { get; set; } = inner;
    public LaptopTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopTemplate? Find(string code)
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

    public void Save(LaptopTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop templates. Empty fields match all.</summary>
public sealed record LaptopTemplateQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopTemplate> Apply(List<LaptopTemplate> items)
    {
        var matched = Collections.Filter(items, (LaptopTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop template store.</summary>
public static class LaptopTemplateStore
{
    public static ILaptopTemplateStore NewMemoryLaptopTemplateStore()
    {
        return new MemoryLaptopTemplateStore(new Dictionary<string, LaptopTemplate>(), new List<string>());
    }

    public static ILaptopTemplateStore NewCachedLaptopTemplateStore(MemoryLaptopTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopTemplateStore(inner, LaptopTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopTemplate> FindLaptopTemplates(ILaptopTemplateStore store, LaptopTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
