namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility requests by code.</summary>
public interface IMobilityRequestStore
{
    MobilityRequest? Find(string code);
    void Save(MobilityRequest item);
    bool Remove(string code);
    List<MobilityRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityRequestStore(Dictionary<string, MobilityRequest> items, List<string> order) : IMobilityRequestStore
{
    public Dictionary<string, MobilityRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityRequest item)
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

    public List<MobilityRequest> List()
    {
        var outValue = new List<MobilityRequest>();
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
public sealed class CachedMobilityRequestStore(MemoryMobilityRequestStore inner, MobilityRequestCache cache, int hits, int misses) : IMobilityRequestStore
{
    public MemoryMobilityRequestStore Inner { get; set; } = inner;
    public MobilityRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityRequest? Find(string code)
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

    public void Save(MobilityRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility requests. Empty fields match all.</summary>
public sealed record MobilityRequestQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityRequest> Apply(List<MobilityRequest> items)
    {
        var matched = Collections.Filter(items, (MobilityRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility request store.</summary>
public static class MobilityRequestStore
{
    public static IMobilityRequestStore NewMemoryMobilityRequestStore()
    {
        return new MemoryMobilityRequestStore(new Dictionary<string, MobilityRequest>(), new List<string>());
    }

    public static IMobilityRequestStore NewCachedMobilityRequestStore(MemoryMobilityRequestStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityRequestStore(inner, MobilityRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilityRequest> FindMobilityRequests(IMobilityRequestStore store, MobilityRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
