namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending access badge requests by code.</summary>
public interface IPendingAccessBadgeRequestStore
{
    PendingAccessBadgeRequest? Find(string code);
    void Save(PendingAccessBadgeRequest item);
    bool Remove(string code);
    List<PendingAccessBadgeRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAccessBadgeRequestStore(Dictionary<string, PendingAccessBadgeRequest> items, List<string> order) : IPendingAccessBadgeRequestStore
{
    public Dictionary<string, PendingAccessBadgeRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAccessBadgeRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAccessBadgeRequest item)
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

    public List<PendingAccessBadgeRequest> List()
    {
        var outValue = new List<PendingAccessBadgeRequest>();
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
public sealed class CachedPendingAccessBadgeRequestStore(MemoryPendingAccessBadgeRequestStore inner, PendingAccessBadgeRequestCache cache, int hits, int misses) : IPendingAccessBadgeRequestStore
{
    public MemoryPendingAccessBadgeRequestStore Inner { get; set; } = inner;
    public PendingAccessBadgeRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAccessBadgeRequest? Find(string code)
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

    public void Save(PendingAccessBadgeRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAccessBadgeRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending access badge requests. Empty fields match all.</summary>
public sealed record PendingAccessBadgeRequestQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAccessBadgeRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<PendingAccessBadgeRequest> Apply(List<PendingAccessBadgeRequest> items)
    {
        var matched = Collections.Filter(items, (PendingAccessBadgeRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending access badge request store.</summary>
public static class PendingAccessBadgeRequestStore
{
    public static IPendingAccessBadgeRequestStore NewMemoryPendingAccessBadgeRequestStore()
    {
        return new MemoryPendingAccessBadgeRequestStore(
            new Dictionary<string, PendingAccessBadgeRequest>(),
            new List<string>());
    }

    public static IPendingAccessBadgeRequestStore NewCachedPendingAccessBadgeRequestStore(MemoryPendingAccessBadgeRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAccessBadgeRequestStore(
            inner,
            PendingAccessBadgeRequestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingAccessBadgeRequest> FindPendingAccessBadgeRequests(IPendingAccessBadgeRequestStore store, PendingAccessBadgeRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
