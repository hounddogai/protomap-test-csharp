namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict assignment baselines by code.</summary>
public interface IStrictAssignmentBaselineStore
{
    StrictAssignmentBaseline? Find(string code);
    void Save(StrictAssignmentBaseline item);
    bool Remove(string code);
    List<StrictAssignmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssignmentBaselineStore(Dictionary<string, StrictAssignmentBaseline> items, List<string> order) : IStrictAssignmentBaselineStore
{
    public Dictionary<string, StrictAssignmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssignmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssignmentBaseline item)
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

    public List<StrictAssignmentBaseline> List()
    {
        var outValue = new List<StrictAssignmentBaseline>();
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
public sealed class CachedStrictAssignmentBaselineStore(MemoryStrictAssignmentBaselineStore inner, StrictAssignmentBaselineCache cache, int hits, int misses) : IStrictAssignmentBaselineStore
{
    public MemoryStrictAssignmentBaselineStore Inner { get; set; } = inner;
    public StrictAssignmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssignmentBaseline? Find(string code)
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

    public void Save(StrictAssignmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssignmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict assignment baselines. Empty fields match all.</summary>
public sealed record StrictAssignmentBaselineQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssignmentBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssignmentBaseline> Apply(List<StrictAssignmentBaseline> items)
    {
        var matched = Collections.Filter(items, (StrictAssignmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict assignment baseline store.</summary>
public static class StrictAssignmentBaselineStore
{
    public static IStrictAssignmentBaselineStore NewMemoryStrictAssignmentBaselineStore()
    {
        return new MemoryStrictAssignmentBaselineStore(
            new Dictionary<string, StrictAssignmentBaseline>(),
            new List<string>());
    }

    public static IStrictAssignmentBaselineStore NewCachedStrictAssignmentBaselineStore(MemoryStrictAssignmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssignmentBaselineStore(
            inner,
            StrictAssignmentBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictAssignmentBaseline> FindStrictAssignmentBaselines(IStrictAssignmentBaselineStore store, StrictAssignmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
