namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment thresholds by code.</summary>
public interface IAssignmentThresholdStore
{
    AssignmentThreshold? Find(string code);
    void Save(AssignmentThreshold item);
    bool Remove(string code);
    List<AssignmentThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentThresholdStore(Dictionary<string, AssignmentThreshold> items, List<string> order) : IAssignmentThresholdStore
{
    public Dictionary<string, AssignmentThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentThreshold item)
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

    public List<AssignmentThreshold> List()
    {
        var outValue = new List<AssignmentThreshold>();
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
public sealed class CachedAssignmentThresholdStore(MemoryAssignmentThresholdStore inner, AssignmentThresholdCache cache, int hits, int misses) : IAssignmentThresholdStore
{
    public MemoryAssignmentThresholdStore Inner { get; set; } = inner;
    public AssignmentThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentThreshold? Find(string code)
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

    public void Save(AssignmentThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment thresholds. Empty fields match all.</summary>
public sealed record AssignmentThresholdQuery(string Label, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentThreshold> Apply(List<AssignmentThreshold> items)
    {
        var matched = Collections.Filter(items, (AssignmentThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment threshold store.</summary>
public static class AssignmentThresholdStore
{
    public static IAssignmentThresholdStore NewMemoryAssignmentThresholdStore()
    {
        return new MemoryAssignmentThresholdStore(new Dictionary<string, AssignmentThreshold>(), new List<string>());
    }

    public static IAssignmentThresholdStore NewCachedAssignmentThresholdStore(MemoryAssignmentThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentThresholdStore(inner, AssignmentThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentThreshold> FindAssignmentThresholds(IAssignmentThresholdStore store, AssignmentThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
