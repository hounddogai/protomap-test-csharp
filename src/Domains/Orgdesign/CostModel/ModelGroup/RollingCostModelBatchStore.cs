namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling cost model batches by code.</summary>
public interface IRollingCostModelBatchStore
{
    RollingCostModelBatch? Find(string code);
    void Save(RollingCostModelBatch item);
    bool Remove(string code);
    List<RollingCostModelBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingCostModelBatchStore(Dictionary<string, RollingCostModelBatch> items, List<string> order) : IRollingCostModelBatchStore
{
    public Dictionary<string, RollingCostModelBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingCostModelBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingCostModelBatch item)
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

    public List<RollingCostModelBatch> List()
    {
        var outValue = new List<RollingCostModelBatch>();
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
public sealed class CachedRollingCostModelBatchStore(MemoryRollingCostModelBatchStore inner, RollingCostModelBatchCache cache, int hits, int misses) : IRollingCostModelBatchStore
{
    public MemoryRollingCostModelBatchStore Inner { get; set; } = inner;
    public RollingCostModelBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingCostModelBatch? Find(string code)
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

    public void Save(RollingCostModelBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingCostModelBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling cost model batches. Empty fields match all.</summary>
public sealed record RollingCostModelBatchQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingCostModelBatch item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingCostModelBatch> Apply(List<RollingCostModelBatch> items)
    {
        var matched = Collections.Filter(items, (RollingCostModelBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling cost model batch store.</summary>
public static class RollingCostModelBatchStore
{
    public static IRollingCostModelBatchStore NewMemoryRollingCostModelBatchStore()
    {
        return new MemoryRollingCostModelBatchStore(
            new Dictionary<string, RollingCostModelBatch>(),
            new List<string>());
    }

    public static IRollingCostModelBatchStore NewCachedRollingCostModelBatchStore(MemoryRollingCostModelBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRollingCostModelBatchStore(inner, RollingCostModelBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingCostModelBatch> FindRollingCostModelBatches(IRollingCostModelBatchStore store, RollingCostModelBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
