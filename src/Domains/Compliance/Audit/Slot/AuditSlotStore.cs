namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit slots by code.</summary>
public interface IAuditSlotStore
{
    AuditSlot? Find(string code);
    void Save(AuditSlot item);
    bool Remove(string code);
    List<AuditSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditSlotStore(Dictionary<string, AuditSlot> items, List<string> order) : IAuditSlotStore
{
    public Dictionary<string, AuditSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditSlot item)
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

    public List<AuditSlot> List()
    {
        var outValue = new List<AuditSlot>();
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
public sealed class CachedAuditSlotStore(MemoryAuditSlotStore inner, AuditSlotCache cache, int hits, int misses) : IAuditSlotStore
{
    public MemoryAuditSlotStore Inner { get; set; } = inner;
    public AuditSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditSlot? Find(string code)
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

    public void Save(AuditSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit slots. Empty fields match all.</summary>
public sealed record AuditSlotQuery(string Unit, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AuditSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditSlot> Apply(List<AuditSlot> items)
    {
        var matched = Collections.Filter(items, (AuditSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit slot store.</summary>
public static class AuditSlotStore
{
    public static IAuditSlotStore NewMemoryAuditSlotStore()
    {
        return new MemoryAuditSlotStore(new Dictionary<string, AuditSlot>(), new List<string>());
    }

    public static IAuditSlotStore NewCachedAuditSlotStore(MemoryAuditSlotStore inner, int ttl, IClock clock)
    {
        return new CachedAuditSlotStore(inner, AuditSlotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditSlot> FindAuditSlots(IAuditSlotStore store, AuditSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
