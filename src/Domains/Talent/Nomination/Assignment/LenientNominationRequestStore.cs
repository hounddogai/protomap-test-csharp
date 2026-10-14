namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient nomination requests by code.</summary>
public interface ILenientNominationRequestStore
{
    LenientNominationRequest? Find(string code);
    void Save(LenientNominationRequest item);
    bool Remove(string code);
    List<LenientNominationRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientNominationRequestStore(Dictionary<string, LenientNominationRequest> items, List<string> order) : ILenientNominationRequestStore
{
    public Dictionary<string, LenientNominationRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientNominationRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientNominationRequest item)
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

    public List<LenientNominationRequest> List()
    {
        var outValue = new List<LenientNominationRequest>();
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
public sealed class CachedLenientNominationRequestStore(MemoryLenientNominationRequestStore inner, LenientNominationRequestCache cache, int hits, int misses) : ILenientNominationRequestStore
{
    public MemoryLenientNominationRequestStore Inner { get; set; } = inner;
    public LenientNominationRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientNominationRequest? Find(string code)
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

    public void Save(LenientNominationRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientNominationRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient nomination requests. Empty fields match all.</summary>
public sealed record LenientNominationRequestQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LenientNominationRequest item)
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

    public List<LenientNominationRequest> Apply(List<LenientNominationRequest> items)
    {
        var matched = Collections.Filter(items, (LenientNominationRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient nomination request store.</summary>
public static class LenientNominationRequestStore
{
    public static ILenientNominationRequestStore NewMemoryLenientNominationRequestStore()
    {
        return new MemoryLenientNominationRequestStore(
            new Dictionary<string, LenientNominationRequest>(),
            new List<string>());
    }

    public static ILenientNominationRequestStore NewCachedLenientNominationRequestStore(MemoryLenientNominationRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientNominationRequestStore(
            inner,
            LenientNominationRequestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientNominationRequest> FindLenientNominationRequests(ILenientNominationRequestStore store, LenientNominationRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
