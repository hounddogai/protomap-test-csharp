namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched town hall notices by code.</summary>
public interface IBatchedTownHallNoticeStore
{
    BatchedTownHallNotice? Find(string code);
    void Save(BatchedTownHallNotice item);
    bool Remove(string code);
    List<BatchedTownHallNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTownHallNoticeStore(Dictionary<string, BatchedTownHallNotice> items, List<string> order) : IBatchedTownHallNoticeStore
{
    public Dictionary<string, BatchedTownHallNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTownHallNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTownHallNotice item)
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

    public List<BatchedTownHallNotice> List()
    {
        var outValue = new List<BatchedTownHallNotice>();
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
public sealed class CachedBatchedTownHallNoticeStore(MemoryBatchedTownHallNoticeStore inner, BatchedTownHallNoticeCache cache, int hits, int misses) : IBatchedTownHallNoticeStore
{
    public MemoryBatchedTownHallNoticeStore Inner { get; set; } = inner;
    public BatchedTownHallNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTownHallNotice? Find(string code)
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

    public void Save(BatchedTownHallNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTownHallNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched town hall notices. Empty fields match all.</summary>
public sealed record BatchedTownHallNoticeQuery(string Unit, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTownHallNotice item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<BatchedTownHallNotice> Apply(List<BatchedTownHallNotice> items)
    {
        var matched = Collections.Filter(items, (BatchedTownHallNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched town hall notice store.</summary>
public static class BatchedTownHallNoticeStore
{
    public static IBatchedTownHallNoticeStore NewMemoryBatchedTownHallNoticeStore()
    {
        return new MemoryBatchedTownHallNoticeStore(
            new Dictionary<string, BatchedTownHallNotice>(),
            new List<string>());
    }

    public static IBatchedTownHallNoticeStore NewCachedBatchedTownHallNoticeStore(MemoryBatchedTownHallNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTownHallNoticeStore(inner, BatchedTownHallNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedTownHallNotice> FindBatchedTownHallNotices(IBatchedTownHallNoticeStore store, BatchedTownHallNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
