namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched vendor templates by code.</summary>
public interface IBatchedVendorTemplateStore
{
    BatchedVendorTemplate? Find(string code);
    void Save(BatchedVendorTemplate item);
    bool Remove(string code);
    List<BatchedVendorTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedVendorTemplateStore(Dictionary<string, BatchedVendorTemplate> items, List<string> order) : IBatchedVendorTemplateStore
{
    public Dictionary<string, BatchedVendorTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedVendorTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedVendorTemplate item)
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

    public List<BatchedVendorTemplate> List()
    {
        var outValue = new List<BatchedVendorTemplate>();
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
public sealed class CachedBatchedVendorTemplateStore(MemoryBatchedVendorTemplateStore inner, BatchedVendorTemplateCache cache, int hits, int misses) : IBatchedVendorTemplateStore
{
    public MemoryBatchedVendorTemplateStore Inner { get; set; } = inner;
    public BatchedVendorTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedVendorTemplate? Find(string code)
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

    public void Save(BatchedVendorTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedVendorTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched vendor templates. Empty fields match all.</summary>
public sealed record BatchedVendorTemplateQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedVendorTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedVendorTemplate> Apply(List<BatchedVendorTemplate> items)
    {
        var matched = Collections.Filter(items, (BatchedVendorTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched vendor template store.</summary>
public static class BatchedVendorTemplateStore
{
    public static IBatchedVendorTemplateStore NewMemoryBatchedVendorTemplateStore()
    {
        return new MemoryBatchedVendorTemplateStore(
            new Dictionary<string, BatchedVendorTemplate>(),
            new List<string>());
    }

    public static IBatchedVendorTemplateStore NewCachedBatchedVendorTemplateStore(MemoryBatchedVendorTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedVendorTemplateStore(inner, BatchedVendorTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedVendorTemplate> FindBatchedVendorTemplates(IBatchedVendorTemplateStore store, BatchedVendorTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
