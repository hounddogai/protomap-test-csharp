namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default span profiles by code.</summary>
public interface IDefaultSpanProfileStore
{
    DefaultSpanProfile? Find(string code);
    void Save(DefaultSpanProfile item);
    bool Remove(string code);
    List<DefaultSpanProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSpanProfileStore(Dictionary<string, DefaultSpanProfile> items, List<string> order) : IDefaultSpanProfileStore
{
    public Dictionary<string, DefaultSpanProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSpanProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSpanProfile item)
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

    public List<DefaultSpanProfile> List()
    {
        var outValue = new List<DefaultSpanProfile>();
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
public sealed class CachedDefaultSpanProfileStore(MemoryDefaultSpanProfileStore inner, DefaultSpanProfileCache cache, int hits, int misses) : IDefaultSpanProfileStore
{
    public MemoryDefaultSpanProfileStore Inner { get; set; } = inner;
    public DefaultSpanProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSpanProfile? Find(string code)
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

    public void Save(DefaultSpanProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSpanProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default span profiles. Empty fields match all.</summary>
public sealed record DefaultSpanProfileQuery(string Department, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSpanProfile item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSpanProfile> Apply(List<DefaultSpanProfile> items)
    {
        var matched = Collections.Filter(items, (DefaultSpanProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default span profile store.</summary>
public static class DefaultSpanProfileStore
{
    public static IDefaultSpanProfileStore NewMemoryDefaultSpanProfileStore()
    {
        return new MemoryDefaultSpanProfileStore(new Dictionary<string, DefaultSpanProfile>(), new List<string>());
    }

    public static IDefaultSpanProfileStore NewCachedDefaultSpanProfileStore(MemoryDefaultSpanProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSpanProfileStore(inner, DefaultSpanProfileCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultSpanProfile> FindDefaultSpanProfiles(IDefaultSpanProfileStore store, DefaultSpanProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
