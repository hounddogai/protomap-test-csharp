namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional holiday policies by code.</summary>
public interface IRegionalHolidayPolicyStore
{
    RegionalHolidayPolicy? Find(string code);
    void Save(RegionalHolidayPolicy item);
    bool Remove(string code);
    List<RegionalHolidayPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalHolidayPolicyStore(Dictionary<string, RegionalHolidayPolicy> items, List<string> order) : IRegionalHolidayPolicyStore
{
    public Dictionary<string, RegionalHolidayPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalHolidayPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalHolidayPolicy item)
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

    public List<RegionalHolidayPolicy> List()
    {
        var outValue = new List<RegionalHolidayPolicy>();
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
public sealed class CachedRegionalHolidayPolicyStore(MemoryRegionalHolidayPolicyStore inner, RegionalHolidayPolicyCache cache, int hits, int misses) : IRegionalHolidayPolicyStore
{
    public MemoryRegionalHolidayPolicyStore Inner { get; set; } = inner;
    public RegionalHolidayPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalHolidayPolicy? Find(string code)
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

    public void Save(RegionalHolidayPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalHolidayPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional holiday policies. Empty fields match all.</summary>
public sealed record RegionalHolidayPolicyQuery(string Channel, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalHolidayPolicy item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalHolidayPolicy> Apply(List<RegionalHolidayPolicy> items)
    {
        var matched = Collections.Filter(items, (RegionalHolidayPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional holiday policy store.</summary>
public static class RegionalHolidayPolicyStore
{
    public static IRegionalHolidayPolicyStore NewMemoryRegionalHolidayPolicyStore()
    {
        return new MemoryRegionalHolidayPolicyStore(
            new Dictionary<string, RegionalHolidayPolicy>(),
            new List<string>());
    }

    public static IRegionalHolidayPolicyStore NewCachedRegionalHolidayPolicyStore(MemoryRegionalHolidayPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalHolidayPolicyStore(
            inner,
            RegionalHolidayPolicyCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalHolidayPolicy> FindRegionalHolidayPolicies(IRegionalHolidayPolicyStore store, RegionalHolidayPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
