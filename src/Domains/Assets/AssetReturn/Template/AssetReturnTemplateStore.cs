namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return templates by code.</summary>
public interface IAssetReturnTemplateStore
{
    AssetReturnTemplate? Find(string code);
    void Save(AssetReturnTemplate item);
    bool Remove(string code);
    List<AssetReturnTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnTemplateStore(Dictionary<string, AssetReturnTemplate> items, List<string> order) : IAssetReturnTemplateStore
{
    public Dictionary<string, AssetReturnTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnTemplate item)
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

    public List<AssetReturnTemplate> List()
    {
        var outValue = new List<AssetReturnTemplate>();
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
public sealed class CachedAssetReturnTemplateStore(MemoryAssetReturnTemplateStore inner, AssetReturnTemplateCache cache, int hits, int misses) : IAssetReturnTemplateStore
{
    public MemoryAssetReturnTemplateStore Inner { get; set; } = inner;
    public AssetReturnTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnTemplate? Find(string code)
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

    public void Save(AssetReturnTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return templates. Empty fields match all.</summary>
public sealed record AssetReturnTemplateQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnTemplate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnTemplate> Apply(List<AssetReturnTemplate> items)
    {
        var matched = Collections.Filter(items, (AssetReturnTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return template store.</summary>
public static class AssetReturnTemplateStore
{
    public static IAssetReturnTemplateStore NewMemoryAssetReturnTemplateStore()
    {
        return new MemoryAssetReturnTemplateStore(new Dictionary<string, AssetReturnTemplate>(), new List<string>());
    }

    public static IAssetReturnTemplateStore NewCachedAssetReturnTemplateStore(MemoryAssetReturnTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnTemplateStore(inner, AssetReturnTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssetReturnTemplate> FindAssetReturnTemplates(IAssetReturnTemplateStore store, AssetReturnTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
