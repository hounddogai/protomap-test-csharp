namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy headcount assignments by code.</summary>
public interface ILegacyHeadcountAssignmentStore
{
    LegacyHeadcountAssignment? Find(string code);
    void Save(LegacyHeadcountAssignment item);
    bool Remove(string code);
    List<LegacyHeadcountAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyHeadcountAssignmentStore(Dictionary<string, LegacyHeadcountAssignment> items, List<string> order) : ILegacyHeadcountAssignmentStore
{
    public Dictionary<string, LegacyHeadcountAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyHeadcountAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyHeadcountAssignment item)
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

    public List<LegacyHeadcountAssignment> List()
    {
        var outValue = new List<LegacyHeadcountAssignment>();
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
public sealed class CachedLegacyHeadcountAssignmentStore(MemoryLegacyHeadcountAssignmentStore inner, LegacyHeadcountAssignmentCache cache, int hits, int misses) : ILegacyHeadcountAssignmentStore
{
    public MemoryLegacyHeadcountAssignmentStore Inner { get; set; } = inner;
    public LegacyHeadcountAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyHeadcountAssignment? Find(string code)
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

    public void Save(LegacyHeadcountAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyHeadcountAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy headcount assignments. Empty fields match all.</summary>
public sealed record LegacyHeadcountAssignmentQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyHeadcountAssignment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyHeadcountAssignment> Apply(List<LegacyHeadcountAssignment> items)
    {
        var matched = Collections.Filter(items, (LegacyHeadcountAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy headcount assignment store.</summary>
public static class LegacyHeadcountAssignmentStore
{
    public static ILegacyHeadcountAssignmentStore NewMemoryLegacyHeadcountAssignmentStore()
    {
        return new MemoryLegacyHeadcountAssignmentStore(
            new Dictionary<string, LegacyHeadcountAssignment>(),
            new List<string>());
    }

    public static ILegacyHeadcountAssignmentStore NewCachedLegacyHeadcountAssignmentStore(MemoryLegacyHeadcountAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyHeadcountAssignmentStore(
            inner,
            LegacyHeadcountAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyHeadcountAssignment> FindLegacyHeadcountAssignments(ILegacyHeadcountAssignmentStore store, LegacyHeadcountAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
