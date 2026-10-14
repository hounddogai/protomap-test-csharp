namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default desk policies by code.</summary>
public interface IDefaultDeskPolicyStore
{
    DefaultDeskPolicy? Find(string code);
    void Save(DefaultDeskPolicy item);
    bool Remove(string code);
    List<DefaultDeskPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultDeskPolicyStore(Dictionary<string, DefaultDeskPolicy> items, List<string> order) : IDefaultDeskPolicyStore
{
    public Dictionary<string, DefaultDeskPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultDeskPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultDeskPolicy item)
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

    public List<DefaultDeskPolicy> List()
    {
        var outValue = new List<DefaultDeskPolicy>();
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
public sealed class CachedDefaultDeskPolicyStore(MemoryDefaultDeskPolicyStore inner, DefaultDeskPolicyCache cache, int hits, int misses) : IDefaultDeskPolicyStore
{
    public MemoryDefaultDeskPolicyStore Inner { get; set; } = inner;
    public DefaultDeskPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultDeskPolicy? Find(string code)
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

    public void Save(DefaultDeskPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultDeskPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default desk policies. Empty fields match all.</summary>
public sealed record DefaultDeskPolicyQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultDeskPolicy item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<DefaultDeskPolicy> Apply(List<DefaultDeskPolicy> items)
    {
        var matched = Collections.Filter(items, (DefaultDeskPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default desk policy store.</summary>
public static class DefaultDeskPolicyStore
{
    public static IDefaultDeskPolicyStore NewMemoryDefaultDeskPolicyStore()
    {
        return new MemoryDefaultDeskPolicyStore(new Dictionary<string, DefaultDeskPolicy>(), new List<string>());
    }

    public static IDefaultDeskPolicyStore NewCachedDefaultDeskPolicyStore(MemoryDefaultDeskPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultDeskPolicyStore(inner, DefaultDeskPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultDeskPolicy> FindDefaultDeskPolicies(IDefaultDeskPolicyStore store, DefaultDeskPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
