namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling bonus pool slots by code.</summary>
public interface IRollingBonusPoolSlotStore
{
    RollingBonusPoolSlot? Find(string code);
    void Save(RollingBonusPoolSlot item);
    bool Remove(string code);
    List<RollingBonusPoolSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingBonusPoolSlotStore(Dictionary<string, RollingBonusPoolSlot> items, List<string> order) : IRollingBonusPoolSlotStore
{
    public Dictionary<string, RollingBonusPoolSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingBonusPoolSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingBonusPoolSlot item)
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

    public List<RollingBonusPoolSlot> List()
    {
        var outValue = new List<RollingBonusPoolSlot>();
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
public sealed class CachedRollingBonusPoolSlotStore(MemoryRollingBonusPoolSlotStore inner, RollingBonusPoolSlotCache cache, int hits, int misses) : IRollingBonusPoolSlotStore
{
    public MemoryRollingBonusPoolSlotStore Inner { get; set; } = inner;
    public RollingBonusPoolSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingBonusPoolSlot? Find(string code)
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

    public void Save(RollingBonusPoolSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingBonusPoolSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling bonus pool slots. Empty fields match all.</summary>
public sealed record RollingBonusPoolSlotQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingBonusPoolSlot item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RollingBonusPoolSlot> Apply(List<RollingBonusPoolSlot> items)
    {
        var matched = Collections.Filter(items, (RollingBonusPoolSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling bonus pool slot store.</summary>
public static class RollingBonusPoolSlotStore
{
    public static IRollingBonusPoolSlotStore NewMemoryRollingBonusPoolSlotStore()
    {
        return new MemoryRollingBonusPoolSlotStore(
            new Dictionary<string, RollingBonusPoolSlot>(),
            new List<string>());
    }

    public static IRollingBonusPoolSlotStore NewCachedRollingBonusPoolSlotStore(MemoryRollingBonusPoolSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRollingBonusPoolSlotStore(inner, RollingBonusPoolSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RollingBonusPoolSlot> FindRollingBonusPoolSlots(IRollingBonusPoolSlotStore store, RollingBonusPoolSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
