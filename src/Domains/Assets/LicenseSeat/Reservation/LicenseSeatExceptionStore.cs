namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat exceptions by code.</summary>
public interface ILicenseSeatExceptionStore
{
    LicenseSeatException? Find(string code);
    void Save(LicenseSeatException item);
    bool Remove(string code);
    List<LicenseSeatException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatExceptionStore(Dictionary<string, LicenseSeatException> items, List<string> order) : ILicenseSeatExceptionStore
{
    public Dictionary<string, LicenseSeatException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatException item)
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

    public List<LicenseSeatException> List()
    {
        var outValue = new List<LicenseSeatException>();
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
public sealed class CachedLicenseSeatExceptionStore(MemoryLicenseSeatExceptionStore inner, LicenseSeatExceptionCache cache, int hits, int misses) : ILicenseSeatExceptionStore
{
    public MemoryLicenseSeatExceptionStore Inner { get; set; } = inner;
    public LicenseSeatExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatException? Find(string code)
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

    public void Save(LicenseSeatException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat exceptions. Empty fields match all.</summary>
public sealed record LicenseSeatExceptionQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatException item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatException> Apply(List<LicenseSeatException> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat exception store.</summary>
public static class LicenseSeatExceptionStore
{
    public static ILicenseSeatExceptionStore NewMemoryLicenseSeatExceptionStore()
    {
        return new MemoryLicenseSeatExceptionStore(
            new Dictionary<string, LicenseSeatException>(),
            new List<string>());
    }

    public static ILicenseSeatExceptionStore NewCachedLicenseSeatExceptionStore(MemoryLicenseSeatExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatExceptionStore(inner, LicenseSeatExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatException> FindLicenseSeatExceptions(ILicenseSeatExceptionStore store, LicenseSeatExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
