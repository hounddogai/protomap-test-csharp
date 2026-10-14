namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback laptop templates by code.</summary>
public interface IFallbackLaptopTemplateStore
{
    FallbackLaptopTemplate? Find(string code);
    void Save(FallbackLaptopTemplate item);
    bool Remove(string code);
    List<FallbackLaptopTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLaptopTemplateStore(Dictionary<string, FallbackLaptopTemplate> items, List<string> order) : IFallbackLaptopTemplateStore
{
    public Dictionary<string, FallbackLaptopTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLaptopTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLaptopTemplate item)
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

    public List<FallbackLaptopTemplate> List()
    {
        var outValue = new List<FallbackLaptopTemplate>();
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
public sealed class CachedFallbackLaptopTemplateStore(MemoryFallbackLaptopTemplateStore inner, FallbackLaptopTemplateCache cache, int hits, int misses) : IFallbackLaptopTemplateStore
{
    public MemoryFallbackLaptopTemplateStore Inner { get; set; } = inner;
    public FallbackLaptopTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLaptopTemplate? Find(string code)
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

    public void Save(FallbackLaptopTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLaptopTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback laptop templates. Empty fields match all.</summary>
public sealed record FallbackLaptopTemplateQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLaptopTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLaptopTemplate> Apply(List<FallbackLaptopTemplate> items)
    {
        var matched = Collections.Filter(items, (FallbackLaptopTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback laptop template store.</summary>
public static class FallbackLaptopTemplateStore
{
    public static IFallbackLaptopTemplateStore NewMemoryFallbackLaptopTemplateStore()
    {
        return new MemoryFallbackLaptopTemplateStore(
            new Dictionary<string, FallbackLaptopTemplate>(),
            new List<string>());
    }

    public static IFallbackLaptopTemplateStore NewCachedFallbackLaptopTemplateStore(MemoryFallbackLaptopTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLaptopTemplateStore(
            inner,
            FallbackLaptopTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLaptopTemplate> FindFallbackLaptopTemplates(IFallbackLaptopTemplateStore store, FallbackLaptopTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
