namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday cycles by code.</summary>
public interface IHolidayCycleStore
{
    HolidayCycle? Find(string code);
    void Save(HolidayCycle item);
    bool Remove(string code);
    List<HolidayCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayCycleStore(Dictionary<string, HolidayCycle> items, List<string> order) : IHolidayCycleStore
{
    public Dictionary<string, HolidayCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayCycle item)
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

    public List<HolidayCycle> List()
    {
        var outValue = new List<HolidayCycle>();
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
public sealed class CachedHolidayCycleStore(MemoryHolidayCycleStore inner, HolidayCycleCache cache, int hits, int misses) : IHolidayCycleStore
{
    public MemoryHolidayCycleStore Inner { get; set; } = inner;
    public HolidayCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayCycle? Find(string code)
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

    public void Save(HolidayCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday cycles. Empty fields match all.</summary>
public sealed record HolidayCycleQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayCycle> Apply(List<HolidayCycle> items)
    {
        var matched = Collections.Filter(items, (HolidayCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday cycle store.</summary>
public static class HolidayCycleStore
{
    public static IHolidayCycleStore NewMemoryHolidayCycleStore()
    {
        return new MemoryHolidayCycleStore(new Dictionary<string, HolidayCycle>(), new List<string>());
    }

    public static IHolidayCycleStore NewCachedHolidayCycleStore(MemoryHolidayCycleStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayCycleStore(inner, HolidayCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayCycle> FindHolidayCycles(IHolidayCycleStore store, HolidayCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
