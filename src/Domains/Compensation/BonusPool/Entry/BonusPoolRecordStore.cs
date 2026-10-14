namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool records by code.</summary>
public interface IBonusPoolRecordStore
{
    BonusPoolRecord? Find(string code);
    void Save(BonusPoolRecord item);
    bool Remove(string code);
    List<BonusPoolRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolRecordStore(Dictionary<string, BonusPoolRecord> items, List<string> order) : IBonusPoolRecordStore
{
    public Dictionary<string, BonusPoolRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolRecord item)
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

    public List<BonusPoolRecord> List()
    {
        var outValue = new List<BonusPoolRecord>();
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
public sealed class CachedBonusPoolRecordStore(MemoryBonusPoolRecordStore inner, BonusPoolRecordCache cache, int hits, int misses) : IBonusPoolRecordStore
{
    public MemoryBonusPoolRecordStore Inner { get; set; } = inner;
    public BonusPoolRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolRecord? Find(string code)
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

    public void Save(BonusPoolRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool records. Empty fields match all.</summary>
public sealed record BonusPoolRecordQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolRecord item)
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

    public List<BonusPoolRecord> Apply(List<BonusPoolRecord> items)
    {
        var matched = Collections.Filter(items, (BonusPoolRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool record store.</summary>
public static class BonusPoolRecordStore
{
    public static IBonusPoolRecordStore NewMemoryBonusPoolRecordStore()
    {
        return new MemoryBonusPoolRecordStore(new Dictionary<string, BonusPoolRecord>(), new List<string>());
    }

    public static IBonusPoolRecordStore NewCachedBonusPoolRecordStore(MemoryBonusPoolRecordStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolRecordStore(inner, BonusPoolRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BonusPoolRecord> FindBonusPoolRecords(IBonusPoolRecordStore store, BonusPoolRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
