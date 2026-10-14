namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday policies by code.</summary>
public interface IHolidayPolicyStore
{
    HolidayPolicy? Find(string code);
    void Save(HolidayPolicy item);
    bool Remove(string code);
    List<HolidayPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayPolicyStore(Dictionary<string, HolidayPolicy> items, List<string> order) : IHolidayPolicyStore
{
    public Dictionary<string, HolidayPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayPolicy item)
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

    public List<HolidayPolicy> List()
    {
        var outValue = new List<HolidayPolicy>();
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
public sealed class CachedHolidayPolicyStore(MemoryHolidayPolicyStore inner, HolidayPolicyCache cache, int hits, int misses) : IHolidayPolicyStore
{
    public MemoryHolidayPolicyStore Inner { get; set; } = inner;
    public HolidayPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayPolicy? Find(string code)
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

    public void Save(HolidayPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday policies. Empty fields match all.</summary>
public sealed record HolidayPolicyQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayPolicy item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<HolidayPolicy> Apply(List<HolidayPolicy> items)
    {
        var matched = Collections.Filter(items, (HolidayPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday policy store.</summary>
public static class HolidayPolicyStore
{
    public static IHolidayPolicyStore NewMemoryHolidayPolicyStore()
    {
        return new MemoryHolidayPolicyStore(new Dictionary<string, HolidayPolicy>(), new List<string>());
    }

    public static IHolidayPolicyStore NewCachedHolidayPolicyStore(MemoryHolidayPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayPolicyStore(inner, HolidayPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayPolicy> FindHolidayPolicies(IHolidayPolicyStore store, HolidayPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
