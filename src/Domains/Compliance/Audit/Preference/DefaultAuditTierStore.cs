namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default audit tiers by code.</summary>
public interface IDefaultAuditTierStore
{
    DefaultAuditTier? Find(string code);
    void Save(DefaultAuditTier item);
    bool Remove(string code);
    List<DefaultAuditTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAuditTierStore(Dictionary<string, DefaultAuditTier> items, List<string> order) : IDefaultAuditTierStore
{
    public Dictionary<string, DefaultAuditTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAuditTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAuditTier item)
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

    public List<DefaultAuditTier> List()
    {
        var outValue = new List<DefaultAuditTier>();
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
public sealed class CachedDefaultAuditTierStore(MemoryDefaultAuditTierStore inner, DefaultAuditTierCache cache, int hits, int misses) : IDefaultAuditTierStore
{
    public MemoryDefaultAuditTierStore Inner { get; set; } = inner;
    public DefaultAuditTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAuditTier? Find(string code)
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

    public void Save(DefaultAuditTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAuditTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default audit tiers. Empty fields match all.</summary>
public sealed record DefaultAuditTierQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAuditTier item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAuditTier> Apply(List<DefaultAuditTier> items)
    {
        var matched = Collections.Filter(items, (DefaultAuditTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default audit tier store.</summary>
public static class DefaultAuditTierStore
{
    public static IDefaultAuditTierStore NewMemoryDefaultAuditTierStore()
    {
        return new MemoryDefaultAuditTierStore(new Dictionary<string, DefaultAuditTier>(), new List<string>());
    }

    public static IDefaultAuditTierStore NewCachedDefaultAuditTierStore(MemoryDefaultAuditTierStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAuditTierStore(inner, DefaultAuditTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultAuditTier> FindDefaultAuditTiers(IDefaultAuditTierStore store, DefaultAuditTierQuery query)
    {
        return query.Apply(store.List());
    }
}
