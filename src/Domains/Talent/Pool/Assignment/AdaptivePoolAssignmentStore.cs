namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive pool assignments by code.</summary>
public interface IAdaptivePoolAssignmentStore
{
    AdaptivePoolAssignment? Find(string code);
    void Save(AdaptivePoolAssignment item);
    bool Remove(string code);
    List<AdaptivePoolAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePoolAssignmentStore(Dictionary<string, AdaptivePoolAssignment> items, List<string> order) : IAdaptivePoolAssignmentStore
{
    public Dictionary<string, AdaptivePoolAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePoolAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePoolAssignment item)
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

    public List<AdaptivePoolAssignment> List()
    {
        var outValue = new List<AdaptivePoolAssignment>();
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
public sealed class CachedAdaptivePoolAssignmentStore(MemoryAdaptivePoolAssignmentStore inner, AdaptivePoolAssignmentCache cache, int hits, int misses) : IAdaptivePoolAssignmentStore
{
    public MemoryAdaptivePoolAssignmentStore Inner { get; set; } = inner;
    public AdaptivePoolAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePoolAssignment? Find(string code)
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

    public void Save(AdaptivePoolAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePoolAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive pool assignments. Empty fields match all.</summary>
public sealed record AdaptivePoolAssignmentQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePoolAssignment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AdaptivePoolAssignment> Apply(List<AdaptivePoolAssignment> items)
    {
        var matched = Collections.Filter(items, (AdaptivePoolAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive pool assignment store.</summary>
public static class AdaptivePoolAssignmentStore
{
    public static IAdaptivePoolAssignmentStore NewMemoryAdaptivePoolAssignmentStore()
    {
        return new MemoryAdaptivePoolAssignmentStore(
            new Dictionary<string, AdaptivePoolAssignment>(),
            new List<string>());
    }

    public static IAdaptivePoolAssignmentStore NewCachedAdaptivePoolAssignmentStore(MemoryAdaptivePoolAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePoolAssignmentStore(
            inner,
            AdaptivePoolAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptivePoolAssignment> FindAdaptivePoolAssignments(IAdaptivePoolAssignmentStore store, AdaptivePoolAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
