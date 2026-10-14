namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty templates by code.</summary>
public interface IWarrantyTemplateStore
{
    WarrantyTemplate? Find(string code);
    void Save(WarrantyTemplate item);
    bool Remove(string code);
    List<WarrantyTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyTemplateStore(Dictionary<string, WarrantyTemplate> items, List<string> order) : IWarrantyTemplateStore
{
    public Dictionary<string, WarrantyTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyTemplate item)
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

    public List<WarrantyTemplate> List()
    {
        var outValue = new List<WarrantyTemplate>();
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
public sealed class CachedWarrantyTemplateStore(MemoryWarrantyTemplateStore inner, WarrantyTemplateCache cache, int hits, int misses) : IWarrantyTemplateStore
{
    public MemoryWarrantyTemplateStore Inner { get; set; } = inner;
    public WarrantyTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyTemplate? Find(string code)
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

    public void Save(WarrantyTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty templates. Empty fields match all.</summary>
public sealed record WarrantyTemplateQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyTemplate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyTemplate> Apply(List<WarrantyTemplate> items)
    {
        var matched = Collections.Filter(items, (WarrantyTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty template store.</summary>
public static class WarrantyTemplateStore
{
    public static IWarrantyTemplateStore NewMemoryWarrantyTemplateStore()
    {
        return new MemoryWarrantyTemplateStore(new Dictionary<string, WarrantyTemplate>(), new List<string>());
    }

    public static IWarrantyTemplateStore NewCachedWarrantyTemplateStore(MemoryWarrantyTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyTemplateStore(inner, WarrantyTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyTemplate> FindWarrantyTemplates(IWarrantyTemplateStore store, WarrantyTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
