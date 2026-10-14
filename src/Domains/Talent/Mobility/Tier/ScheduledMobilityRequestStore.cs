namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled mobility requests by code.</summary>
public interface IScheduledMobilityRequestStore
{
    ScheduledMobilityRequest? Find(string code);
    void Save(ScheduledMobilityRequest item);
    bool Remove(string code);
    List<ScheduledMobilityRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledMobilityRequestStore(Dictionary<string, ScheduledMobilityRequest> items, List<string> order) : IScheduledMobilityRequestStore
{
    public Dictionary<string, ScheduledMobilityRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledMobilityRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledMobilityRequest item)
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

    public List<ScheduledMobilityRequest> List()
    {
        var outValue = new List<ScheduledMobilityRequest>();
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
public sealed class CachedScheduledMobilityRequestStore(MemoryScheduledMobilityRequestStore inner, ScheduledMobilityRequestCache cache, int hits, int misses) : IScheduledMobilityRequestStore
{
    public MemoryScheduledMobilityRequestStore Inner { get; set; } = inner;
    public ScheduledMobilityRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledMobilityRequest? Find(string code)
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

    public void Save(ScheduledMobilityRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledMobilityRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled mobility requests. Empty fields match all.</summary>
public sealed record ScheduledMobilityRequestQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledMobilityRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledMobilityRequest> Apply(List<ScheduledMobilityRequest> items)
    {
        var matched = Collections.Filter(items, (ScheduledMobilityRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled mobility request store.</summary>
public static class ScheduledMobilityRequestStore
{
    public static IScheduledMobilityRequestStore NewMemoryScheduledMobilityRequestStore()
    {
        return new MemoryScheduledMobilityRequestStore(
            new Dictionary<string, ScheduledMobilityRequest>(),
            new List<string>());
    }

    public static IScheduledMobilityRequestStore NewCachedScheduledMobilityRequestStore(MemoryScheduledMobilityRequestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledMobilityRequestStore(
            inner,
            ScheduledMobilityRequestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledMobilityRequest> FindScheduledMobilityRequests(IScheduledMobilityRequestStore store, ScheduledMobilityRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
