namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk batches by code.</summary>
public interface IDeskBatchStore
{
    DeskBatch? Find(string code);
    void Save(DeskBatch item);
    bool Remove(string code);
    List<DeskBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskBatchStore(Dictionary<string, DeskBatch> items, List<string> order) : IDeskBatchStore
{
    public Dictionary<string, DeskBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskBatch item)
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

    public List<DeskBatch> List()
    {
        var outValue = new List<DeskBatch>();
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
public sealed class CachedDeskBatchStore(MemoryDeskBatchStore inner, DeskBatchCache cache, int hits, int misses) : IDeskBatchStore
{
    public MemoryDeskBatchStore Inner { get; set; } = inner;
    public DeskBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskBatch? Find(string code)
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

    public void Save(DeskBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk batches. Empty fields match all.</summary>
public sealed record DeskBatchQuery(string Unit, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeskBatch item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskBatch> Apply(List<DeskBatch> items)
    {
        var matched = Collections.Filter(items, (DeskBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk batch store.</summary>
public static class DeskBatchStore
{
    public static IDeskBatchStore NewMemoryDeskBatchStore()
    {
        return new MemoryDeskBatchStore(new Dictionary<string, DeskBatch>(), new List<string>());
    }

    public static IDeskBatchStore NewCachedDeskBatchStore(MemoryDeskBatchStore inner, int ttl, IClock clock)
    {
        return new CachedDeskBatchStore(inner, DeskBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeskBatch> FindDeskBatches(IDeskBatchStore store, DeskBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
