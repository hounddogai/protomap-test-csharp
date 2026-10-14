namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary license seat cycles by code.</summary>
public interface IPrimaryLicenseSeatCycleStore
{
    PrimaryLicenseSeatCycle? Find(string code);
    void Save(PrimaryLicenseSeatCycle item);
    bool Remove(string code);
    List<PrimaryLicenseSeatCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLicenseSeatCycleStore(Dictionary<string, PrimaryLicenseSeatCycle> items, List<string> order) : IPrimaryLicenseSeatCycleStore
{
    public Dictionary<string, PrimaryLicenseSeatCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLicenseSeatCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLicenseSeatCycle item)
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

    public List<PrimaryLicenseSeatCycle> List()
    {
        var outValue = new List<PrimaryLicenseSeatCycle>();
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
public sealed class CachedPrimaryLicenseSeatCycleStore(MemoryPrimaryLicenseSeatCycleStore inner, PrimaryLicenseSeatCycleCache cache, int hits, int misses) : IPrimaryLicenseSeatCycleStore
{
    public MemoryPrimaryLicenseSeatCycleStore Inner { get; set; } = inner;
    public PrimaryLicenseSeatCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLicenseSeatCycle? Find(string code)
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

    public void Save(PrimaryLicenseSeatCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLicenseSeatCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary license seat cycles. Empty fields match all.</summary>
public sealed record PrimaryLicenseSeatCycleQuery(string Department, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLicenseSeatCycle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryLicenseSeatCycle> Apply(List<PrimaryLicenseSeatCycle> items)
    {
        var matched = Collections.Filter(items, (PrimaryLicenseSeatCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary license seat cycle store.</summary>
public static class PrimaryLicenseSeatCycleStore
{
    public static IPrimaryLicenseSeatCycleStore NewMemoryPrimaryLicenseSeatCycleStore()
    {
        return new MemoryPrimaryLicenseSeatCycleStore(
            new Dictionary<string, PrimaryLicenseSeatCycle>(),
            new List<string>());
    }

    public static IPrimaryLicenseSeatCycleStore NewCachedPrimaryLicenseSeatCycleStore(MemoryPrimaryLicenseSeatCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLicenseSeatCycleStore(
            inner,
            PrimaryLicenseSeatCycleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLicenseSeatCycle> FindPrimaryLicenseSeatCycles(IPrimaryLicenseSeatCycleStore store, PrimaryLicenseSeatCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
