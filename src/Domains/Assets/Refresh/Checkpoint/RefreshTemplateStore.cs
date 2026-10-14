namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh templates by code.</summary>
public interface IRefreshTemplateStore
{
    RefreshTemplate? Find(string code);
    void Save(RefreshTemplate item);
    bool Remove(string code);
    List<RefreshTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshTemplateStore(Dictionary<string, RefreshTemplate> items, List<string> order) : IRefreshTemplateStore
{
    public Dictionary<string, RefreshTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshTemplate item)
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

    public List<RefreshTemplate> List()
    {
        var outValue = new List<RefreshTemplate>();
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
public sealed class CachedRefreshTemplateStore(MemoryRefreshTemplateStore inner, RefreshTemplateCache cache, int hits, int misses) : IRefreshTemplateStore
{
    public MemoryRefreshTemplateStore Inner { get; set; } = inner;
    public RefreshTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshTemplate? Find(string code)
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

    public void Save(RefreshTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh templates. Empty fields match all.</summary>
public sealed record RefreshTemplateQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshTemplate> Apply(List<RefreshTemplate> items)
    {
        var matched = Collections.Filter(items, (RefreshTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh template store.</summary>
public static class RefreshTemplateStore
{
    public static IRefreshTemplateStore NewMemoryRefreshTemplateStore()
    {
        return new MemoryRefreshTemplateStore(new Dictionary<string, RefreshTemplate>(), new List<string>());
    }

    public static IRefreshTemplateStore NewCachedRefreshTemplateStore(MemoryRefreshTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshTemplateStore(inner, RefreshTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshTemplate> FindRefreshTemplates(IRefreshTemplateStore store, RefreshTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
