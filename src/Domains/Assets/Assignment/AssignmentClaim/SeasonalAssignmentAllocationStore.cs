namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal assignment allocations by code.</summary>
public interface ISeasonalAssignmentAllocationStore
{
    SeasonalAssignmentAllocation? Find(string code);
    void Save(SeasonalAssignmentAllocation item);
    bool Remove(string code);
    List<SeasonalAssignmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAssignmentAllocationStore(Dictionary<string, SeasonalAssignmentAllocation> items, List<string> order) : ISeasonalAssignmentAllocationStore
{
    public Dictionary<string, SeasonalAssignmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAssignmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAssignmentAllocation item)
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

    public List<SeasonalAssignmentAllocation> List()
    {
        var outValue = new List<SeasonalAssignmentAllocation>();
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
public sealed class CachedSeasonalAssignmentAllocationStore(MemorySeasonalAssignmentAllocationStore inner, SeasonalAssignmentAllocationCache cache, int hits, int misses) : ISeasonalAssignmentAllocationStore
{
    public MemorySeasonalAssignmentAllocationStore Inner { get; set; } = inner;
    public SeasonalAssignmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAssignmentAllocation? Find(string code)
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

    public void Save(SeasonalAssignmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAssignmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal assignment allocations. Empty fields match all.</summary>
public sealed record SeasonalAssignmentAllocationQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAssignmentAllocation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAssignmentAllocation> Apply(List<SeasonalAssignmentAllocation> items)
    {
        var matched = Collections.Filter(items, (SeasonalAssignmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal assignment allocation store.</summary>
public static class SeasonalAssignmentAllocationStore
{
    public static ISeasonalAssignmentAllocationStore NewMemorySeasonalAssignmentAllocationStore()
    {
        return new MemorySeasonalAssignmentAllocationStore(
            new Dictionary<string, SeasonalAssignmentAllocation>(),
            new List<string>());
    }

    public static ISeasonalAssignmentAllocationStore NewCachedSeasonalAssignmentAllocationStore(MemorySeasonalAssignmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAssignmentAllocationStore(
            inner,
            SeasonalAssignmentAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAssignmentAllocation> FindSeasonalAssignmentAllocations(ISeasonalAssignmentAllocationStore store, SeasonalAssignmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
