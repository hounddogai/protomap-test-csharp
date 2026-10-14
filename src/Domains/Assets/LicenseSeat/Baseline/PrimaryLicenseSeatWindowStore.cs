namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary license seat windows by code.</summary>
public interface IPrimaryLicenseSeatWindowStore
{
    PrimaryLicenseSeatWindow? Find(string code);
    void Save(PrimaryLicenseSeatWindow item);
    bool Remove(string code);
    List<PrimaryLicenseSeatWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLicenseSeatWindowStore(Dictionary<string, PrimaryLicenseSeatWindow> items, List<string> order) : IPrimaryLicenseSeatWindowStore
{
    public Dictionary<string, PrimaryLicenseSeatWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLicenseSeatWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLicenseSeatWindow item)
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

    public List<PrimaryLicenseSeatWindow> List()
    {
        var outValue = new List<PrimaryLicenseSeatWindow>();
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
public sealed class CachedPrimaryLicenseSeatWindowStore(MemoryPrimaryLicenseSeatWindowStore inner, PrimaryLicenseSeatWindowCache cache, int hits, int misses) : IPrimaryLicenseSeatWindowStore
{
    public MemoryPrimaryLicenseSeatWindowStore Inner { get; set; } = inner;
    public PrimaryLicenseSeatWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLicenseSeatWindow? Find(string code)
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

    public void Save(PrimaryLicenseSeatWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLicenseSeatWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary license seat windows. Empty fields match all.</summary>
public sealed record PrimaryLicenseSeatWindowQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLicenseSeatWindow item)
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

    public List<PrimaryLicenseSeatWindow> Apply(List<PrimaryLicenseSeatWindow> items)
    {
        var matched = Collections.Filter(items, (PrimaryLicenseSeatWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary license seat window store.</summary>
public static class PrimaryLicenseSeatWindowStore
{
    public static IPrimaryLicenseSeatWindowStore NewMemoryPrimaryLicenseSeatWindowStore()
    {
        return new MemoryPrimaryLicenseSeatWindowStore(
            new Dictionary<string, PrimaryLicenseSeatWindow>(),
            new List<string>());
    }

    public static IPrimaryLicenseSeatWindowStore NewCachedPrimaryLicenseSeatWindowStore(MemoryPrimaryLicenseSeatWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLicenseSeatWindowStore(
            inner,
            PrimaryLicenseSeatWindowCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLicenseSeatWindow> FindPrimaryLicenseSeatWindows(IPrimaryLicenseSeatWindowStore store, PrimaryLicenseSeatWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
