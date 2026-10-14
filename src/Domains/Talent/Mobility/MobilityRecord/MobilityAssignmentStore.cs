namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility assignments by code.</summary>
public interface IMobilityAssignmentStore
{
    MobilityAssignment? Find(string code);
    void Save(MobilityAssignment item);
    bool Remove(string code);
    List<MobilityAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityAssignmentStore(Dictionary<string, MobilityAssignment> items, List<string> order) : IMobilityAssignmentStore
{
    public Dictionary<string, MobilityAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityAssignment item)
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

    public List<MobilityAssignment> List()
    {
        var outValue = new List<MobilityAssignment>();
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
public sealed class CachedMobilityAssignmentStore(MemoryMobilityAssignmentStore inner, MobilityAssignmentCache cache, int hits, int misses) : IMobilityAssignmentStore
{
    public MemoryMobilityAssignmentStore Inner { get; set; } = inner;
    public MobilityAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityAssignment? Find(string code)
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

    public void Save(MobilityAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility assignments. Empty fields match all.</summary>
public sealed record MobilityAssignmentQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<MobilityAssignment> Apply(List<MobilityAssignment> items)
    {
        var matched = Collections.Filter(items, (MobilityAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility assignment store.</summary>
public static class MobilityAssignmentStore
{
    public static IMobilityAssignmentStore NewMemoryMobilityAssignmentStore()
    {
        return new MemoryMobilityAssignmentStore(new Dictionary<string, MobilityAssignment>(), new List<string>());
    }

    public static IMobilityAssignmentStore NewCachedMobilityAssignmentStore(MemoryMobilityAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityAssignmentStore(inner, MobilityAssignmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MobilityAssignment> FindMobilityAssignments(IMobilityAssignmentStore store, MobilityAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
