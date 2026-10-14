namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling access badge requests by code.</summary>
public interface IRollingAccessBadgeRequestStore
{
    RollingAccessBadgeRequest? Find(string code);
    void Save(RollingAccessBadgeRequest item);
    bool Remove(string code);
    List<RollingAccessBadgeRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccessBadgeRequestStore(Dictionary<string, RollingAccessBadgeRequest> items, List<string> order) : IRollingAccessBadgeRequestStore
{
    public Dictionary<string, RollingAccessBadgeRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccessBadgeRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccessBadgeRequest item)
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

    public List<RollingAccessBadgeRequest> List()
    {
        var outValue = new List<RollingAccessBadgeRequest>();
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
public sealed class CachedRollingAccessBadgeRequestStore(MemoryRollingAccessBadgeRequestStore inner, RollingAccessBadgeRequestCache cache, int hits, int misses) : IRollingAccessBadgeRequestStore
{
    public MemoryRollingAccessBadgeRequestStore Inner { get; set; } = inner;
    public RollingAccessBadgeRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccessBadgeRequest? Find(string code)
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

    public void Save(RollingAccessBadgeRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccessBadgeRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling access badge requests. Empty fields match all.</summary>
public sealed record RollingAccessBadgeRequestQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccessBadgeRequest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<RollingAccessBadgeRequest> Apply(List<RollingAccessBadgeRequest> items)
    {
        var matched = Collections.Filter(items, (RollingAccessBadgeRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling access badge request store.</summary>
public static class RollingAccessBadgeRequestStore
{
    public static IRollingAccessBadgeRequestStore NewMemoryRollingAccessBadgeRequestStore()
    {
        return new MemoryRollingAccessBadgeRequestStore(
            new Dictionary<string, RollingAccessBadgeRequest>(),
            new List<string>());
    }

    public static IRollingAccessBadgeRequestStore NewCachedRollingAccessBadgeRequestStore(MemoryRollingAccessBadgeRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccessBadgeRequestStore(
            inner,
            RollingAccessBadgeRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingAccessBadgeRequest> FindRollingAccessBadgeRequests(IRollingAccessBadgeRequestStore store, RollingAccessBadgeRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
