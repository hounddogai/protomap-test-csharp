namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement requests by code.</summary>
public interface IEngagementRequestStore
{
    EngagementRequest? Find(string code);
    void Save(EngagementRequest item);
    bool Remove(string code);
    List<EngagementRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementRequestStore(Dictionary<string, EngagementRequest> items, List<string> order) : IEngagementRequestStore
{
    public Dictionary<string, EngagementRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementRequest item)
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

    public List<EngagementRequest> List()
    {
        var outValue = new List<EngagementRequest>();
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
public sealed class CachedEngagementRequestStore(MemoryEngagementRequestStore inner, EngagementRequestCache cache, int hits, int misses) : IEngagementRequestStore
{
    public MemoryEngagementRequestStore Inner { get; set; } = inner;
    public EngagementRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementRequest? Find(string code)
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

    public void Save(EngagementRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement requests. Empty fields match all.</summary>
public sealed record EngagementRequestQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementRequest> Apply(List<EngagementRequest> items)
    {
        var matched = Collections.Filter(items, (EngagementRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement request store.</summary>
public static class EngagementRequestStore
{
    public static IEngagementRequestStore NewMemoryEngagementRequestStore()
    {
        return new MemoryEngagementRequestStore(new Dictionary<string, EngagementRequest>(), new List<string>());
    }

    public static IEngagementRequestStore NewCachedEngagementRequestStore(MemoryEngagementRequestStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementRequestStore(inner, EngagementRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementRequest> FindEngagementRequests(IEngagementRequestStore store, EngagementRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
