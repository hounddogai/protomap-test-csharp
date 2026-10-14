namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy cycles by code.</summary>
public interface IOccupancyCycleStore
{
    OccupancyCycle? Find(string code);
    void Save(OccupancyCycle item);
    bool Remove(string code);
    List<OccupancyCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyCycleStore(Dictionary<string, OccupancyCycle> items, List<string> order) : IOccupancyCycleStore
{
    public Dictionary<string, OccupancyCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyCycle item)
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

    public List<OccupancyCycle> List()
    {
        var outValue = new List<OccupancyCycle>();
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
public sealed class CachedOccupancyCycleStore(MemoryOccupancyCycleStore inner, OccupancyCycleCache cache, int hits, int misses) : IOccupancyCycleStore
{
    public MemoryOccupancyCycleStore Inner { get; set; } = inner;
    public OccupancyCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyCycle? Find(string code)
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

    public void Save(OccupancyCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy cycles. Empty fields match all.</summary>
public sealed record OccupancyCycleQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyCycle> Apply(List<OccupancyCycle> items)
    {
        var matched = Collections.Filter(items, (OccupancyCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy cycle store.</summary>
public static class OccupancyCycleStore
{
    public static IOccupancyCycleStore NewMemoryOccupancyCycleStore()
    {
        return new MemoryOccupancyCycleStore(new Dictionary<string, OccupancyCycle>(), new List<string>());
    }

    public static IOccupancyCycleStore NewCachedOccupancyCycleStore(MemoryOccupancyCycleStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyCycleStore(inner, OccupancyCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyCycle> FindOccupancyCycles(IOccupancyCycleStore store, OccupancyCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
