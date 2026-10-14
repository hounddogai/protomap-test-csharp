namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday exceptions by code.</summary>
public interface IHolidayExceptionStore
{
    HolidayException? Find(string code);
    void Save(HolidayException item);
    bool Remove(string code);
    List<HolidayException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayExceptionStore(Dictionary<string, HolidayException> items, List<string> order) : IHolidayExceptionStore
{
    public Dictionary<string, HolidayException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayException item)
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

    public List<HolidayException> List()
    {
        var outValue = new List<HolidayException>();
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
public sealed class CachedHolidayExceptionStore(MemoryHolidayExceptionStore inner, HolidayExceptionCache cache, int hits, int misses) : IHolidayExceptionStore
{
    public MemoryHolidayExceptionStore Inner { get; set; } = inner;
    public HolidayExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayException? Find(string code)
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

    public void Save(HolidayException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday exceptions. Empty fields match all.</summary>
public sealed record HolidayExceptionQuery(string Unit, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayException item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayException> Apply(List<HolidayException> items)
    {
        var matched = Collections.Filter(items, (HolidayException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday exception store.</summary>
public static class HolidayExceptionStore
{
    public static IHolidayExceptionStore NewMemoryHolidayExceptionStore()
    {
        return new MemoryHolidayExceptionStore(new Dictionary<string, HolidayException>(), new List<string>());
    }

    public static IHolidayExceptionStore NewCachedHolidayExceptionStore(MemoryHolidayExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayExceptionStore(inner, HolidayExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayException> FindHolidayExceptions(IHolidayExceptionStore store, HolidayExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
