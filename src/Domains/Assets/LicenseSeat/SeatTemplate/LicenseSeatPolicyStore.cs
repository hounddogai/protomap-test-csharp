namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat policies by code.</summary>
public interface ILicenseSeatPolicyStore
{
    LicenseSeatPolicy? Find(string code);
    void Save(LicenseSeatPolicy item);
    bool Remove(string code);
    List<LicenseSeatPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatPolicyStore(Dictionary<string, LicenseSeatPolicy> items, List<string> order) : ILicenseSeatPolicyStore
{
    public Dictionary<string, LicenseSeatPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatPolicy item)
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

    public List<LicenseSeatPolicy> List()
    {
        var outValue = new List<LicenseSeatPolicy>();
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
public sealed class CachedLicenseSeatPolicyStore(MemoryLicenseSeatPolicyStore inner, LicenseSeatPolicyCache cache, int hits, int misses) : ILicenseSeatPolicyStore
{
    public MemoryLicenseSeatPolicyStore Inner { get; set; } = inner;
    public LicenseSeatPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatPolicy? Find(string code)
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

    public void Save(LicenseSeatPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat policies. Empty fields match all.</summary>
public sealed record LicenseSeatPolicyQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatPolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatPolicy> Apply(List<LicenseSeatPolicy> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat policy store.</summary>
public static class LicenseSeatPolicyStore
{
    public static ILicenseSeatPolicyStore NewMemoryLicenseSeatPolicyStore()
    {
        return new MemoryLicenseSeatPolicyStore(new Dictionary<string, LicenseSeatPolicy>(), new List<string>());
    }

    public static ILicenseSeatPolicyStore NewCachedLicenseSeatPolicyStore(MemoryLicenseSeatPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatPolicyStore(inner, LicenseSeatPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatPolicy> FindLicenseSeatPolicies(ILicenseSeatPolicyStore store, LicenseSeatPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
