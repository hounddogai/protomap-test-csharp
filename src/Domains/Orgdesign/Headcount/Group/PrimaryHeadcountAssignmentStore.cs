namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary headcount assignments by code.</summary>
public interface IPrimaryHeadcountAssignmentStore
{
    PrimaryHeadcountAssignment? Find(string code);
    void Save(PrimaryHeadcountAssignment item);
    bool Remove(string code);
    List<PrimaryHeadcountAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryHeadcountAssignmentStore(Dictionary<string, PrimaryHeadcountAssignment> items, List<string> order) : IPrimaryHeadcountAssignmentStore
{
    public Dictionary<string, PrimaryHeadcountAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryHeadcountAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryHeadcountAssignment item)
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

    public List<PrimaryHeadcountAssignment> List()
    {
        var outValue = new List<PrimaryHeadcountAssignment>();
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
public sealed class CachedPrimaryHeadcountAssignmentStore(MemoryPrimaryHeadcountAssignmentStore inner, PrimaryHeadcountAssignmentCache cache, int hits, int misses) : IPrimaryHeadcountAssignmentStore
{
    public MemoryPrimaryHeadcountAssignmentStore Inner { get; set; } = inner;
    public PrimaryHeadcountAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryHeadcountAssignment? Find(string code)
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

    public void Save(PrimaryHeadcountAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryHeadcountAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary headcount assignments. Empty fields match all.</summary>
public sealed record PrimaryHeadcountAssignmentQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryHeadcountAssignment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryHeadcountAssignment> Apply(List<PrimaryHeadcountAssignment> items)
    {
        var matched = Collections.Filter(items, (PrimaryHeadcountAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary headcount assignment store.</summary>
public static class PrimaryHeadcountAssignmentStore
{
    public static IPrimaryHeadcountAssignmentStore NewMemoryPrimaryHeadcountAssignmentStore()
    {
        return new MemoryPrimaryHeadcountAssignmentStore(
            new Dictionary<string, PrimaryHeadcountAssignment>(),
            new List<string>());
    }

    public static IPrimaryHeadcountAssignmentStore NewCachedPrimaryHeadcountAssignmentStore(MemoryPrimaryHeadcountAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryHeadcountAssignmentStore(
            inner,
            PrimaryHeadcountAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryHeadcountAssignment> FindPrimaryHeadcountAssignments(IPrimaryHeadcountAssignmentStore store, PrimaryHeadcountAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
