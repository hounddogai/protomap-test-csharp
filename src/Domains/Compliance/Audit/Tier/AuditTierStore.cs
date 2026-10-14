namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit tiers by code.</summary>
public interface IAuditTierStore
{
    AuditTier? Find(string code);
    void Save(AuditTier item);
    bool Remove(string code);
    List<AuditTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditTierStore(Dictionary<string, AuditTier> items, List<string> order) : IAuditTierStore
{
    public Dictionary<string, AuditTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditTier item)
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

    public List<AuditTier> List()
    {
        var outValue = new List<AuditTier>();
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
public sealed class CachedAuditTierStore(MemoryAuditTierStore inner, AuditTierCache cache, int hits, int misses) : IAuditTierStore
{
    public MemoryAuditTierStore Inner { get; set; } = inner;
    public AuditTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditTier? Find(string code)
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

    public void Save(AuditTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit tiers. Empty fields match all.</summary>
public sealed record AuditTierQuery(string Team, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AuditTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AuditTier> Apply(List<AuditTier> items)
    {
        var matched = Collections.Filter(items, (AuditTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit tier store.</summary>
public static class AuditTierStore
{
    public static IAuditTierStore NewMemoryAuditTierStore()
    {
        return new MemoryAuditTierStore(new Dictionary<string, AuditTier>(), new List<string>());
    }

    public static IAuditTierStore NewCachedAuditTierStore(MemoryAuditTierStore inner, int ttl, IClock clock)
    {
        return new CachedAuditTierStore(inner, AuditTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditTier> FindAuditTiers(IAuditTierStore store, AuditTierQuery query)
    {
        return query.Apply(store.List());
    }
}
