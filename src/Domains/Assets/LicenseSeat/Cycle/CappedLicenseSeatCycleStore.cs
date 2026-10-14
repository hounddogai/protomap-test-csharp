namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped license seat cycles by code.</summary>
public interface ICappedLicenseSeatCycleStore
{
    CappedLicenseSeatCycle? Find(string code);
    void Save(CappedLicenseSeatCycle item);
    bool Remove(string code);
    List<CappedLicenseSeatCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLicenseSeatCycleStore(Dictionary<string, CappedLicenseSeatCycle> items, List<string> order) : ICappedLicenseSeatCycleStore
{
    public Dictionary<string, CappedLicenseSeatCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLicenseSeatCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLicenseSeatCycle item)
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

    public List<CappedLicenseSeatCycle> List()
    {
        var outValue = new List<CappedLicenseSeatCycle>();
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
public sealed class CachedCappedLicenseSeatCycleStore(MemoryCappedLicenseSeatCycleStore inner, CappedLicenseSeatCycleCache cache, int hits, int misses) : ICappedLicenseSeatCycleStore
{
    public MemoryCappedLicenseSeatCycleStore Inner { get; set; } = inner;
    public CappedLicenseSeatCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLicenseSeatCycle? Find(string code)
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

    public void Save(CappedLicenseSeatCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLicenseSeatCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped license seat cycles. Empty fields match all.</summary>
public sealed record CappedLicenseSeatCycleQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLicenseSeatCycle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<CappedLicenseSeatCycle> Apply(List<CappedLicenseSeatCycle> items)
    {
        var matched = Collections.Filter(items, (CappedLicenseSeatCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped license seat cycle store.</summary>
public static class CappedLicenseSeatCycleStore
{
    public static ICappedLicenseSeatCycleStore NewMemoryCappedLicenseSeatCycleStore()
    {
        return new MemoryCappedLicenseSeatCycleStore(
            new Dictionary<string, CappedLicenseSeatCycle>(),
            new List<string>());
    }

    public static ICappedLicenseSeatCycleStore NewCachedCappedLicenseSeatCycleStore(MemoryCappedLicenseSeatCycleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLicenseSeatCycleStore(
            inner,
            CappedLicenseSeatCycleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedLicenseSeatCycle> FindCappedLicenseSeatCycles(ICappedLicenseSeatCycleStore store, CappedLicenseSeatCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
