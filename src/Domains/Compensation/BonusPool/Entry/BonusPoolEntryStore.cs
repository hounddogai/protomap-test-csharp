namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool entries by code.</summary>
public interface IBonusPoolEntryStore
{
    BonusPoolEntry? Find(string code);
    void Save(BonusPoolEntry item);
    bool Remove(string code);
    List<BonusPoolEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolEntryStore(Dictionary<string, BonusPoolEntry> items, List<string> order) : IBonusPoolEntryStore
{
    public Dictionary<string, BonusPoolEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolEntry item)
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

    public List<BonusPoolEntry> List()
    {
        var outValue = new List<BonusPoolEntry>();
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
public sealed class CachedBonusPoolEntryStore(MemoryBonusPoolEntryStore inner, BonusPoolEntryCache cache, int hits, int misses) : IBonusPoolEntryStore
{
    public MemoryBonusPoolEntryStore Inner { get; set; } = inner;
    public BonusPoolEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolEntry? Find(string code)
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

    public void Save(BonusPoolEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool entries. Empty fields match all.</summary>
public sealed record BonusPoolEntryQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolEntry item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolEntry> Apply(List<BonusPoolEntry> items)
    {
        var matched = Collections.Filter(items, (BonusPoolEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool entry store.</summary>
public static class BonusPoolEntryStore
{
    public static IBonusPoolEntryStore NewMemoryBonusPoolEntryStore()
    {
        return new MemoryBonusPoolEntryStore(new Dictionary<string, BonusPoolEntry>(), new List<string>());
    }

    public static IBonusPoolEntryStore NewCachedBonusPoolEntryStore(MemoryBonusPoolEntryStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolEntryStore(inner, BonusPoolEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolEntry> FindBonusPoolEntries(IBonusPoolEntryStore store, BonusPoolEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
