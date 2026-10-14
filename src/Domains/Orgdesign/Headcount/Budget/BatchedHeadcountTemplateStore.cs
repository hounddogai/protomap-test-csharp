namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched headcount templates by code.</summary>
public interface IBatchedHeadcountTemplateStore
{
    BatchedHeadcountTemplate? Find(string code);
    void Save(BatchedHeadcountTemplate item);
    bool Remove(string code);
    List<BatchedHeadcountTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedHeadcountTemplateStore(Dictionary<string, BatchedHeadcountTemplate> items, List<string> order) : IBatchedHeadcountTemplateStore
{
    public Dictionary<string, BatchedHeadcountTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedHeadcountTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedHeadcountTemplate item)
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

    public List<BatchedHeadcountTemplate> List()
    {
        var outValue = new List<BatchedHeadcountTemplate>();
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
public sealed class CachedBatchedHeadcountTemplateStore(MemoryBatchedHeadcountTemplateStore inner, BatchedHeadcountTemplateCache cache, int hits, int misses) : IBatchedHeadcountTemplateStore
{
    public MemoryBatchedHeadcountTemplateStore Inner { get; set; } = inner;
    public BatchedHeadcountTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedHeadcountTemplate? Find(string code)
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

    public void Save(BatchedHeadcountTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedHeadcountTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched headcount templates. Empty fields match all.</summary>
public sealed record BatchedHeadcountTemplateQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedHeadcountTemplate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<BatchedHeadcountTemplate> Apply(List<BatchedHeadcountTemplate> items)
    {
        var matched = Collections.Filter(items, (BatchedHeadcountTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched headcount template store.</summary>
public static class BatchedHeadcountTemplateStore
{
    public static IBatchedHeadcountTemplateStore NewMemoryBatchedHeadcountTemplateStore()
    {
        return new MemoryBatchedHeadcountTemplateStore(
            new Dictionary<string, BatchedHeadcountTemplate>(),
            new List<string>());
    }

    public static IBatchedHeadcountTemplateStore NewCachedBatchedHeadcountTemplateStore(MemoryBatchedHeadcountTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedHeadcountTemplateStore(
            inner,
            BatchedHeadcountTemplateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedHeadcountTemplate> FindBatchedHeadcountTemplates(IBatchedHeadcountTemplateStore store, BatchedHeadcountTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
