namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall batches by code.</summary>
public interface ITownHallBatchStore
{
    TownHallBatch? Find(string code);
    void Save(TownHallBatch item);
    bool Remove(string code);
    List<TownHallBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallBatchStore(Dictionary<string, TownHallBatch> items, List<string> order) : ITownHallBatchStore
{
    public Dictionary<string, TownHallBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallBatch item)
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

    public List<TownHallBatch> List()
    {
        var outValue = new List<TownHallBatch>();
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
public sealed class CachedTownHallBatchStore(MemoryTownHallBatchStore inner, TownHallBatchCache cache, int hits, int misses) : ITownHallBatchStore
{
    public MemoryTownHallBatchStore Inner { get; set; } = inner;
    public TownHallBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallBatch? Find(string code)
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

    public void Save(TownHallBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall batches. Empty fields match all.</summary>
public sealed record TownHallBatchQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallBatch> Apply(List<TownHallBatch> items)
    {
        var matched = Collections.Filter(items, (TownHallBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall batch store.</summary>
public static class TownHallBatchStore
{
    public static ITownHallBatchStore NewMemoryTownHallBatchStore()
    {
        return new MemoryTownHallBatchStore(new Dictionary<string, TownHallBatch>(), new List<string>());
    }

    public static ITownHallBatchStore NewCachedTownHallBatchStore(MemoryTownHallBatchStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallBatchStore(inner, TownHallBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallBatch> FindTownHallBatches(ITownHallBatchStore store, TownHallBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
