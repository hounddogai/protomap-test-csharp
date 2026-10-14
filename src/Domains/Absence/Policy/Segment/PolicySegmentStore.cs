namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy segments by code.</summary>
public interface IPolicySegmentStore
{
    PolicySegment? Find(string code);
    void Save(PolicySegment item);
    bool Remove(string code);
    List<PolicySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicySegmentStore(Dictionary<string, PolicySegment> items, List<string> order) : IPolicySegmentStore
{
    public Dictionary<string, PolicySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicySegment item)
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

    public List<PolicySegment> List()
    {
        var outValue = new List<PolicySegment>();
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
public sealed class CachedPolicySegmentStore(MemoryPolicySegmentStore inner, PolicySegmentCache cache, int hits, int misses) : IPolicySegmentStore
{
    public MemoryPolicySegmentStore Inner { get; set; } = inner;
    public PolicySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicySegment? Find(string code)
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

    public void Save(PolicySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy segments. Empty fields match all.</summary>
public sealed record PolicySegmentQuery(string Country, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PolicySegment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicySegment> Apply(List<PolicySegment> items)
    {
        var matched = Collections.Filter(items, (PolicySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy segment store.</summary>
public static class PolicySegmentStore
{
    public static IPolicySegmentStore NewMemoryPolicySegmentStore()
    {
        return new MemoryPolicySegmentStore(new Dictionary<string, PolicySegment>(), new List<string>());
    }

    public static IPolicySegmentStore NewCachedPolicySegmentStore(MemoryPolicySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPolicySegmentStore(inner, PolicySegmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PolicySegment> FindPolicySegments(IPolicySegmentStore store, PolicySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
