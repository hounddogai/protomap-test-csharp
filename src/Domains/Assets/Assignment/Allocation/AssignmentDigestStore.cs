namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment digests by code.</summary>
public interface IAssignmentDigestStore
{
    AssignmentDigest? Find(string code);
    void Save(AssignmentDigest item);
    bool Remove(string code);
    List<AssignmentDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentDigestStore(Dictionary<string, AssignmentDigest> items, List<string> order) : IAssignmentDigestStore
{
    public Dictionary<string, AssignmentDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentDigest item)
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

    public List<AssignmentDigest> List()
    {
        var outValue = new List<AssignmentDigest>();
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
public sealed class CachedAssignmentDigestStore(MemoryAssignmentDigestStore inner, AssignmentDigestCache cache, int hits, int misses) : IAssignmentDigestStore
{
    public MemoryAssignmentDigestStore Inner { get; set; } = inner;
    public AssignmentDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentDigest? Find(string code)
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

    public void Save(AssignmentDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment digests. Empty fields match all.</summary>
public sealed record AssignmentDigestQuery(string Country, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<AssignmentDigest> Apply(List<AssignmentDigest> items)
    {
        var matched = Collections.Filter(items, (AssignmentDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment digest store.</summary>
public static class AssignmentDigestStore
{
    public static IAssignmentDigestStore NewMemoryAssignmentDigestStore()
    {
        return new MemoryAssignmentDigestStore(new Dictionary<string, AssignmentDigest>(), new List<string>());
    }

    public static IAssignmentDigestStore NewCachedAssignmentDigestStore(MemoryAssignmentDigestStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentDigestStore(inner, AssignmentDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentDigest> FindAssignmentDigests(IAssignmentDigestStore store, AssignmentDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
