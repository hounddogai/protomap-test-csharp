namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default position quotas by code.</summary>
public interface IDefaultPositionQuotaStore
{
    DefaultPositionQuota? Find(string code);
    void Save(DefaultPositionQuota item);
    bool Remove(string code);
    List<DefaultPositionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPositionQuotaStore(Dictionary<string, DefaultPositionQuota> items, List<string> order) : IDefaultPositionQuotaStore
{
    public Dictionary<string, DefaultPositionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPositionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPositionQuota item)
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

    public List<DefaultPositionQuota> List()
    {
        var outValue = new List<DefaultPositionQuota>();
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
public sealed class CachedDefaultPositionQuotaStore(MemoryDefaultPositionQuotaStore inner, DefaultPositionQuotaCache cache, int hits, int misses) : IDefaultPositionQuotaStore
{
    public MemoryDefaultPositionQuotaStore Inner { get; set; } = inner;
    public DefaultPositionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPositionQuota? Find(string code)
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

    public void Save(DefaultPositionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPositionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default position quotas. Empty fields match all.</summary>
public sealed record DefaultPositionQuotaQuery(string Team, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPositionQuota item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPositionQuota> Apply(List<DefaultPositionQuota> items)
    {
        var matched = Collections.Filter(items, (DefaultPositionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default position quota store.</summary>
public static class DefaultPositionQuotaStore
{
    public static IDefaultPositionQuotaStore NewMemoryDefaultPositionQuotaStore()
    {
        return new MemoryDefaultPositionQuotaStore(
            new Dictionary<string, DefaultPositionQuota>(),
            new List<string>());
    }

    public static IDefaultPositionQuotaStore NewCachedDefaultPositionQuotaStore(MemoryDefaultPositionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPositionQuotaStore(inner, DefaultPositionQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultPositionQuota> FindDefaultPositionQuotas(IDefaultPositionQuotaStore store, DefaultPositionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
