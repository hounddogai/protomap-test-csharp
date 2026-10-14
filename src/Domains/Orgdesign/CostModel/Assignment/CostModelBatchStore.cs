namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model batches by code.</summary>
public interface ICostModelBatchStore
{
    CostModelBatch? Find(string code);
    void Save(CostModelBatch item);
    bool Remove(string code);
    List<CostModelBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelBatchStore(Dictionary<string, CostModelBatch> items, List<string> order) : ICostModelBatchStore
{
    public Dictionary<string, CostModelBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelBatch item)
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

    public List<CostModelBatch> List()
    {
        var outValue = new List<CostModelBatch>();
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
public sealed class CachedCostModelBatchStore(MemoryCostModelBatchStore inner, CostModelBatchCache cache, int hits, int misses) : ICostModelBatchStore
{
    public MemoryCostModelBatchStore Inner { get; set; } = inner;
    public CostModelBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelBatch? Find(string code)
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

    public void Save(CostModelBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model batches. Empty fields match all.</summary>
public sealed record CostModelBatchQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelBatch item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CostModelBatch> Apply(List<CostModelBatch> items)
    {
        var matched = Collections.Filter(items, (CostModelBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model batch store.</summary>
public static class CostModelBatchStore
{
    public static ICostModelBatchStore NewMemoryCostModelBatchStore()
    {
        return new MemoryCostModelBatchStore(new Dictionary<string, CostModelBatch>(), new List<string>());
    }

    public static ICostModelBatchStore NewCachedCostModelBatchStore(MemoryCostModelBatchStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelBatchStore(inner, CostModelBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CostModelBatch> FindCostModelBatches(ICostModelBatchStore store, CostModelBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
