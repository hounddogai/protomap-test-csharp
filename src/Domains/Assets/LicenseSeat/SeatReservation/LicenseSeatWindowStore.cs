namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat windows by code.</summary>
public interface ILicenseSeatWindowStore
{
    LicenseSeatWindow? Find(string code);
    void Save(LicenseSeatWindow item);
    bool Remove(string code);
    List<LicenseSeatWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatWindowStore(Dictionary<string, LicenseSeatWindow> items, List<string> order) : ILicenseSeatWindowStore
{
    public Dictionary<string, LicenseSeatWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatWindow item)
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

    public List<LicenseSeatWindow> List()
    {
        var outValue = new List<LicenseSeatWindow>();
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
public sealed class CachedLicenseSeatWindowStore(MemoryLicenseSeatWindowStore inner, LicenseSeatWindowCache cache, int hits, int misses) : ILicenseSeatWindowStore
{
    public MemoryLicenseSeatWindowStore Inner { get; set; } = inner;
    public LicenseSeatWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatWindow? Find(string code)
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

    public void Save(LicenseSeatWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat windows. Empty fields match all.</summary>
public sealed record LicenseSeatWindowQuery(string Department, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatWindow item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LicenseSeatWindow> Apply(List<LicenseSeatWindow> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat window store.</summary>
public static class LicenseSeatWindowStore
{
    public static ILicenseSeatWindowStore NewMemoryLicenseSeatWindowStore()
    {
        return new MemoryLicenseSeatWindowStore(new Dictionary<string, LicenseSeatWindow>(), new List<string>());
    }

    public static ILicenseSeatWindowStore NewCachedLicenseSeatWindowStore(MemoryLicenseSeatWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatWindowStore(inner, LicenseSeatWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatWindow> FindLicenseSeatWindows(ILicenseSeatWindowStore store, LicenseSeatWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
