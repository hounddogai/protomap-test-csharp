namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment baselines by code.</summary>
public interface IAssignmentBaselineStore
{
    AssignmentBaseline? Find(string code);
    void Save(AssignmentBaseline item);
    bool Remove(string code);
    List<AssignmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentBaselineStore(Dictionary<string, AssignmentBaseline> items, List<string> order) : IAssignmentBaselineStore
{
    public Dictionary<string, AssignmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentBaseline item)
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

    public List<AssignmentBaseline> List()
    {
        var outValue = new List<AssignmentBaseline>();
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
public sealed class CachedAssignmentBaselineStore(MemoryAssignmentBaselineStore inner, AssignmentBaselineCache cache, int hits, int misses) : IAssignmentBaselineStore
{
    public MemoryAssignmentBaselineStore Inner { get; set; } = inner;
    public AssignmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentBaseline? Find(string code)
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

    public void Save(AssignmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment baselines. Empty fields match all.</summary>
public sealed record AssignmentBaselineQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentBaseline> Apply(List<AssignmentBaseline> items)
    {
        var matched = Collections.Filter(items, (AssignmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment baseline store.</summary>
public static class AssignmentBaselineStore
{
    public static IAssignmentBaselineStore NewMemoryAssignmentBaselineStore()
    {
        return new MemoryAssignmentBaselineStore(new Dictionary<string, AssignmentBaseline>(), new List<string>());
    }

    public static IAssignmentBaselineStore NewCachedAssignmentBaselineStore(MemoryAssignmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentBaselineStore(inner, AssignmentBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentBaseline> FindAssignmentBaselines(IAssignmentBaselineStore store, AssignmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
