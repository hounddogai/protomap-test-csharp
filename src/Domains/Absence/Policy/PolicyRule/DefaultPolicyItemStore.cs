namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default policy items by code.</summary>
public interface IDefaultPolicyItemStore
{
    DefaultPolicyItem? Find(string code);
    void Save(DefaultPolicyItem item);
    bool Remove(string code);
    List<DefaultPolicyItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPolicyItemStore(Dictionary<string, DefaultPolicyItem> items, List<string> order) : IDefaultPolicyItemStore
{
    public Dictionary<string, DefaultPolicyItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPolicyItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPolicyItem item)
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

    public List<DefaultPolicyItem> List()
    {
        var outValue = new List<DefaultPolicyItem>();
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
public sealed class CachedDefaultPolicyItemStore(MemoryDefaultPolicyItemStore inner, DefaultPolicyItemCache cache, int hits, int misses) : IDefaultPolicyItemStore
{
    public MemoryDefaultPolicyItemStore Inner { get; set; } = inner;
    public DefaultPolicyItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPolicyItem? Find(string code)
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

    public void Save(DefaultPolicyItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPolicyItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default policy items. Empty fields match all.</summary>
public sealed record DefaultPolicyItemQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPolicyItem item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPolicyItem> Apply(List<DefaultPolicyItem> items)
    {
        var matched = Collections.Filter(items, (DefaultPolicyItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default policy item store.</summary>
public static class DefaultPolicyItemStore
{
    public static IDefaultPolicyItemStore NewMemoryDefaultPolicyItemStore()
    {
        return new MemoryDefaultPolicyItemStore(new Dictionary<string, DefaultPolicyItem>(), new List<string>());
    }

    public static IDefaultPolicyItemStore NewCachedDefaultPolicyItemStore(MemoryDefaultPolicyItemStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPolicyItemStore(inner, DefaultPolicyItemCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultPolicyItem> FindDefaultPolicyItems(IDefaultPolicyItemStore store, DefaultPolicyItemQuery query)
    {
        return query.Apply(store.List());
    }
}
