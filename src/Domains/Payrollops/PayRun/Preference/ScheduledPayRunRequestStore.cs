namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pay run requests by code.</summary>
public interface IScheduledPayRunRequestStore
{
    ScheduledPayRunRequest? Find(string code);
    void Save(ScheduledPayRunRequest item);
    bool Remove(string code);
    List<ScheduledPayRunRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayRunRequestStore(Dictionary<string, ScheduledPayRunRequest> items, List<string> order) : IScheduledPayRunRequestStore
{
    public Dictionary<string, ScheduledPayRunRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayRunRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayRunRequest item)
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

    public List<ScheduledPayRunRequest> List()
    {
        var outValue = new List<ScheduledPayRunRequest>();
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
public sealed class CachedScheduledPayRunRequestStore(MemoryScheduledPayRunRequestStore inner, ScheduledPayRunRequestCache cache, int hits, int misses) : IScheduledPayRunRequestStore
{
    public MemoryScheduledPayRunRequestStore Inner { get; set; } = inner;
    public ScheduledPayRunRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayRunRequest? Find(string code)
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

    public void Save(ScheduledPayRunRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayRunRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pay run requests. Empty fields match all.</summary>
public sealed record ScheduledPayRunRequestQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayRunRequest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayRunRequest> Apply(List<ScheduledPayRunRequest> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayRunRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pay run request store.</summary>
public static class ScheduledPayRunRequestStore
{
    public static IScheduledPayRunRequestStore NewMemoryScheduledPayRunRequestStore()
    {
        return new MemoryScheduledPayRunRequestStore(
            new Dictionary<string, ScheduledPayRunRequest>(),
            new List<string>());
    }

    public static IScheduledPayRunRequestStore NewCachedScheduledPayRunRequestStore(MemoryScheduledPayRunRequestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayRunRequestStore(
            inner,
            ScheduledPayRunRequestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPayRunRequest> FindScheduledPayRunRequests(IScheduledPayRunRequestStore store, ScheduledPayRunRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
