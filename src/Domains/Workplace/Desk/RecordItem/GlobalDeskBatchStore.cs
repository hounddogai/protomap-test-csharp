namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global desk batches by code.</summary>
public interface IGlobalDeskBatchStore
{
    GlobalDeskBatch? Find(string code);
    void Save(GlobalDeskBatch item);
    bool Remove(string code);
    List<GlobalDeskBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalDeskBatchStore(Dictionary<string, GlobalDeskBatch> items, List<string> order) : IGlobalDeskBatchStore
{
    public Dictionary<string, GlobalDeskBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalDeskBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalDeskBatch item)
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

    public List<GlobalDeskBatch> List()
    {
        var outValue = new List<GlobalDeskBatch>();
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
public sealed class CachedGlobalDeskBatchStore(MemoryGlobalDeskBatchStore inner, GlobalDeskBatchCache cache, int hits, int misses) : IGlobalDeskBatchStore
{
    public MemoryGlobalDeskBatchStore Inner { get; set; } = inner;
    public GlobalDeskBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalDeskBatch? Find(string code)
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

    public void Save(GlobalDeskBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalDeskBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global desk batches. Empty fields match all.</summary>
public sealed record GlobalDeskBatchQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalDeskBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<GlobalDeskBatch> Apply(List<GlobalDeskBatch> items)
    {
        var matched = Collections.Filter(items, (GlobalDeskBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global desk batch store.</summary>
public static class GlobalDeskBatchStore
{
    public static IGlobalDeskBatchStore NewMemoryGlobalDeskBatchStore()
    {
        return new MemoryGlobalDeskBatchStore(new Dictionary<string, GlobalDeskBatch>(), new List<string>());
    }

    public static IGlobalDeskBatchStore NewCachedGlobalDeskBatchStore(MemoryGlobalDeskBatchStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalDeskBatchStore(inner, GlobalDeskBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalDeskBatch> FindGlobalDeskBatches(IGlobalDeskBatchStore store, GlobalDeskBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
