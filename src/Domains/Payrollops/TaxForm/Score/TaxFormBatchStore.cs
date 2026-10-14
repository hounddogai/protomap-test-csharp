namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form batches by code.</summary>
public interface ITaxFormBatchStore
{
    TaxFormBatch? Find(string code);
    void Save(TaxFormBatch item);
    bool Remove(string code);
    List<TaxFormBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormBatchStore(Dictionary<string, TaxFormBatch> items, List<string> order) : ITaxFormBatchStore
{
    public Dictionary<string, TaxFormBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormBatch item)
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

    public List<TaxFormBatch> List()
    {
        var outValue = new List<TaxFormBatch>();
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
public sealed class CachedTaxFormBatchStore(MemoryTaxFormBatchStore inner, TaxFormBatchCache cache, int hits, int misses) : ITaxFormBatchStore
{
    public MemoryTaxFormBatchStore Inner { get; set; } = inner;
    public TaxFormBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormBatch? Find(string code)
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

    public void Save(TaxFormBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form batches. Empty fields match all.</summary>
public sealed record TaxFormBatchQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TaxFormBatch> Apply(List<TaxFormBatch> items)
    {
        var matched = Collections.Filter(items, (TaxFormBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form batch store.</summary>
public static class TaxFormBatchStore
{
    public static ITaxFormBatchStore NewMemoryTaxFormBatchStore()
    {
        return new MemoryTaxFormBatchStore(new Dictionary<string, TaxFormBatch>(), new List<string>());
    }

    public static ITaxFormBatchStore NewCachedTaxFormBatchStore(MemoryTaxFormBatchStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormBatchStore(inner, TaxFormBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormBatch> FindTaxFormBatches(ITaxFormBatchStore store, TaxFormBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
