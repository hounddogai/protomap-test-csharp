namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday agreements by code.</summary>
public interface IHolidayAgreementStore
{
    HolidayAgreement? Find(string code);
    void Save(HolidayAgreement item);
    bool Remove(string code);
    List<HolidayAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayAgreementStore(Dictionary<string, HolidayAgreement> items, List<string> order) : IHolidayAgreementStore
{
    public Dictionary<string, HolidayAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayAgreement item)
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

    public List<HolidayAgreement> List()
    {
        var outValue = new List<HolidayAgreement>();
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
public sealed class CachedHolidayAgreementStore(MemoryHolidayAgreementStore inner, HolidayAgreementCache cache, int hits, int misses) : IHolidayAgreementStore
{
    public MemoryHolidayAgreementStore Inner { get; set; } = inner;
    public HolidayAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayAgreement? Find(string code)
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

    public void Save(HolidayAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday agreements. Empty fields match all.</summary>
public sealed record HolidayAgreementQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayAgreement> Apply(List<HolidayAgreement> items)
    {
        var matched = Collections.Filter(items, (HolidayAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday agreement store.</summary>
public static class HolidayAgreementStore
{
    public static IHolidayAgreementStore NewMemoryHolidayAgreementStore()
    {
        return new MemoryHolidayAgreementStore(new Dictionary<string, HolidayAgreement>(), new List<string>());
    }

    public static IHolidayAgreementStore NewCachedHolidayAgreementStore(MemoryHolidayAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayAgreementStore(inner, HolidayAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayAgreement> FindHolidayAgreements(IHolidayAgreementStore store, HolidayAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
