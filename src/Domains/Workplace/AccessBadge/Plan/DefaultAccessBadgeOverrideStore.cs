namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default access badge overrides by code.</summary>
public interface IDefaultAccessBadgeOverrideStore
{
    DefaultAccessBadgeOverride? Find(string code);
    void Save(DefaultAccessBadgeOverride item);
    bool Remove(string code);
    List<DefaultAccessBadgeOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAccessBadgeOverrideStore(Dictionary<string, DefaultAccessBadgeOverride> items, List<string> order) : IDefaultAccessBadgeOverrideStore
{
    public Dictionary<string, DefaultAccessBadgeOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAccessBadgeOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAccessBadgeOverride item)
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

    public List<DefaultAccessBadgeOverride> List()
    {
        var outValue = new List<DefaultAccessBadgeOverride>();
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
public sealed class CachedDefaultAccessBadgeOverrideStore(MemoryDefaultAccessBadgeOverrideStore inner, DefaultAccessBadgeOverrideCache cache, int hits, int misses) : IDefaultAccessBadgeOverrideStore
{
    public MemoryDefaultAccessBadgeOverrideStore Inner { get; set; } = inner;
    public DefaultAccessBadgeOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAccessBadgeOverride? Find(string code)
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

    public void Save(DefaultAccessBadgeOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAccessBadgeOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default access badge overrides. Empty fields match all.</summary>
public sealed record DefaultAccessBadgeOverrideQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAccessBadgeOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAccessBadgeOverride> Apply(List<DefaultAccessBadgeOverride> items)
    {
        var matched = Collections.Filter(items, (DefaultAccessBadgeOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default access badge override store.</summary>
public static class DefaultAccessBadgeOverrideStore
{
    public static IDefaultAccessBadgeOverrideStore NewMemoryDefaultAccessBadgeOverrideStore()
    {
        return new MemoryDefaultAccessBadgeOverrideStore(
            new Dictionary<string, DefaultAccessBadgeOverride>(),
            new List<string>());
    }

    public static IDefaultAccessBadgeOverrideStore NewCachedDefaultAccessBadgeOverrideStore(MemoryDefaultAccessBadgeOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAccessBadgeOverrideStore(
            inner,
            DefaultAccessBadgeOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAccessBadgeOverride> FindDefaultAccessBadgeOverrides(IDefaultAccessBadgeOverrideStore store, DefaultAccessBadgeOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
