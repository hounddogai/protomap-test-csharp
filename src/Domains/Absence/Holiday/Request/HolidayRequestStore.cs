namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday requests by code.</summary>
public interface IHolidayRequestStore
{
    HolidayRequest? Find(string code);
    void Save(HolidayRequest item);
    bool Remove(string code);
    List<HolidayRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayRequestStore(Dictionary<string, HolidayRequest> items, List<string> order) : IHolidayRequestStore
{
    public Dictionary<string, HolidayRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayRequest item)
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

    public List<HolidayRequest> List()
    {
        var outValue = new List<HolidayRequest>();
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
public sealed class CachedHolidayRequestStore(MemoryHolidayRequestStore inner, HolidayRequestCache cache, int hits, int misses) : IHolidayRequestStore
{
    public MemoryHolidayRequestStore Inner { get; set; } = inner;
    public HolidayRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayRequest? Find(string code)
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

    public void Save(HolidayRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday requests. Empty fields match all.</summary>
public sealed record HolidayRequestQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayRequest> Apply(List<HolidayRequest> items)
    {
        var matched = Collections.Filter(items, (HolidayRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday request store.</summary>
public static class HolidayRequestStore
{
    public static IHolidayRequestStore NewMemoryHolidayRequestStore()
    {
        return new MemoryHolidayRequestStore(new Dictionary<string, HolidayRequest>(), new List<string>());
    }

    public static IHolidayRequestStore NewCachedHolidayRequestStore(MemoryHolidayRequestStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayRequestStore(inner, HolidayRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<HolidayRequest> FindHolidayRequests(IHolidayRequestStore store, HolidayRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
