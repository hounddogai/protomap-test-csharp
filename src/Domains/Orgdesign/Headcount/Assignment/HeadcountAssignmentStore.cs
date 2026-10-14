namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount assignments by code.</summary>
public interface IHeadcountAssignmentStore
{
    HeadcountAssignment? Find(string code);
    void Save(HeadcountAssignment item);
    bool Remove(string code);
    List<HeadcountAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountAssignmentStore(Dictionary<string, HeadcountAssignment> items, List<string> order) : IHeadcountAssignmentStore
{
    public Dictionary<string, HeadcountAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountAssignment item)
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

    public List<HeadcountAssignment> List()
    {
        var outValue = new List<HeadcountAssignment>();
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
public sealed class CachedHeadcountAssignmentStore(MemoryHeadcountAssignmentStore inner, HeadcountAssignmentCache cache, int hits, int misses) : IHeadcountAssignmentStore
{
    public MemoryHeadcountAssignmentStore Inner { get; set; } = inner;
    public HeadcountAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountAssignment? Find(string code)
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

    public void Save(HeadcountAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount assignments. Empty fields match all.</summary>
public sealed record HeadcountAssignmentQuery(string Team, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountAssignment> Apply(List<HeadcountAssignment> items)
    {
        var matched = Collections.Filter(items, (HeadcountAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount assignment store.</summary>
public static class HeadcountAssignmentStore
{
    public static IHeadcountAssignmentStore NewMemoryHeadcountAssignmentStore()
    {
        return new MemoryHeadcountAssignmentStore(new Dictionary<string, HeadcountAssignment>(), new List<string>());
    }

    public static IHeadcountAssignmentStore NewCachedHeadcountAssignmentStore(MemoryHeadcountAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountAssignmentStore(inner, HeadcountAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountAssignment> FindHeadcountAssignments(IHeadcountAssignmentStore store, HeadcountAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
