namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient mobility requests by code.</summary>
public interface ILenientMobilityRequestStore
{
    LenientMobilityRequest? Find(string code);
    void Save(LenientMobilityRequest item);
    bool Remove(string code);
    List<LenientMobilityRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientMobilityRequestStore(Dictionary<string, LenientMobilityRequest> items, List<string> order) : ILenientMobilityRequestStore
{
    public Dictionary<string, LenientMobilityRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientMobilityRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientMobilityRequest item)
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

    public List<LenientMobilityRequest> List()
    {
        var outValue = new List<LenientMobilityRequest>();
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
public sealed class CachedLenientMobilityRequestStore(MemoryLenientMobilityRequestStore inner, LenientMobilityRequestCache cache, int hits, int misses) : ILenientMobilityRequestStore
{
    public MemoryLenientMobilityRequestStore Inner { get; set; } = inner;
    public LenientMobilityRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientMobilityRequest? Find(string code)
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

    public void Save(LenientMobilityRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientMobilityRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient mobility requests. Empty fields match all.</summary>
public sealed record LenientMobilityRequestQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientMobilityRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientMobilityRequest> Apply(List<LenientMobilityRequest> items)
    {
        var matched = Collections.Filter(items, (LenientMobilityRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient mobility request store.</summary>
public static class LenientMobilityRequestStore
{
    public static ILenientMobilityRequestStore NewMemoryLenientMobilityRequestStore()
    {
        return new MemoryLenientMobilityRequestStore(
            new Dictionary<string, LenientMobilityRequest>(),
            new List<string>());
    }

    public static ILenientMobilityRequestStore NewCachedLenientMobilityRequestStore(MemoryLenientMobilityRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientMobilityRequestStore(
            inner,
            LenientMobilityRequestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientMobilityRequest> FindLenientMobilityRequests(ILenientMobilityRequestStore store, LenientMobilityRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
