namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered cost model assignments by code.</summary>
public interface ITieredCostModelAssignmentStore
{
    TieredCostModelAssignment? Find(string code);
    void Save(TieredCostModelAssignment item);
    bool Remove(string code);
    List<TieredCostModelAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredCostModelAssignmentStore(Dictionary<string, TieredCostModelAssignment> items, List<string> order) : ITieredCostModelAssignmentStore
{
    public Dictionary<string, TieredCostModelAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredCostModelAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredCostModelAssignment item)
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

    public List<TieredCostModelAssignment> List()
    {
        var outValue = new List<TieredCostModelAssignment>();
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
public sealed class CachedTieredCostModelAssignmentStore(MemoryTieredCostModelAssignmentStore inner, TieredCostModelAssignmentCache cache, int hits, int misses) : ITieredCostModelAssignmentStore
{
    public MemoryTieredCostModelAssignmentStore Inner { get; set; } = inner;
    public TieredCostModelAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredCostModelAssignment? Find(string code)
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

    public void Save(TieredCostModelAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredCostModelAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered cost model assignments. Empty fields match all.</summary>
public sealed record TieredCostModelAssignmentQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredCostModelAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredCostModelAssignment> Apply(List<TieredCostModelAssignment> items)
    {
        var matched = Collections.Filter(items, (TieredCostModelAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered cost model assignment store.</summary>
public static class TieredCostModelAssignmentStore
{
    public static ITieredCostModelAssignmentStore NewMemoryTieredCostModelAssignmentStore()
    {
        return new MemoryTieredCostModelAssignmentStore(
            new Dictionary<string, TieredCostModelAssignment>(),
            new List<string>());
    }

    public static ITieredCostModelAssignmentStore NewCachedTieredCostModelAssignmentStore(MemoryTieredCostModelAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedTieredCostModelAssignmentStore(
            inner,
            TieredCostModelAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredCostModelAssignment> FindTieredCostModelAssignments(ITieredCostModelAssignmentStore store, TieredCostModelAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
