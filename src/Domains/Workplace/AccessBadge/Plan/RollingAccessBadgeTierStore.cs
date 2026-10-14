namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling access badge tiers by code.</summary>
public interface IRollingAccessBadgeTierStore
{
    RollingAccessBadgeTier? Find(string code);
    void Save(RollingAccessBadgeTier item);
    bool Remove(string code);
    List<RollingAccessBadgeTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccessBadgeTierStore(Dictionary<string, RollingAccessBadgeTier> items, List<string> order) : IRollingAccessBadgeTierStore
{
    public Dictionary<string, RollingAccessBadgeTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccessBadgeTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccessBadgeTier item)
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

    public List<RollingAccessBadgeTier> List()
    {
        var outValue = new List<RollingAccessBadgeTier>();
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
public sealed class CachedRollingAccessBadgeTierStore(MemoryRollingAccessBadgeTierStore inner, RollingAccessBadgeTierCache cache, int hits, int misses) : IRollingAccessBadgeTierStore
{
    public MemoryRollingAccessBadgeTierStore Inner { get; set; } = inner;
    public RollingAccessBadgeTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccessBadgeTier? Find(string code)
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

    public void Save(RollingAccessBadgeTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccessBadgeTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling access badge tiers. Empty fields match all.</summary>
public sealed record RollingAccessBadgeTierQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccessBadgeTier item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAccessBadgeTier> Apply(List<RollingAccessBadgeTier> items)
    {
        var matched = Collections.Filter(items, (RollingAccessBadgeTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling access badge tier store.</summary>
public static class RollingAccessBadgeTierStore
{
    public static IRollingAccessBadgeTierStore NewMemoryRollingAccessBadgeTierStore()
    {
        return new MemoryRollingAccessBadgeTierStore(
            new Dictionary<string, RollingAccessBadgeTier>(),
            new List<string>());
    }

    public static IRollingAccessBadgeTierStore NewCachedRollingAccessBadgeTierStore(MemoryRollingAccessBadgeTierStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccessBadgeTierStore(
            inner,
            RollingAccessBadgeTierCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingAccessBadgeTier> FindRollingAccessBadgeTiers(IRollingAccessBadgeTierStore store, RollingAccessBadgeTierQuery query)
    {
        return query.Apply(store.List());
    }
}
