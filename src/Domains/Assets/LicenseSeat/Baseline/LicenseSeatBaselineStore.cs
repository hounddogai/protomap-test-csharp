namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat baselines by code.</summary>
public interface ILicenseSeatBaselineStore
{
    LicenseSeatBaseline? Find(string code);
    void Save(LicenseSeatBaseline item);
    bool Remove(string code);
    List<LicenseSeatBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatBaselineStore(Dictionary<string, LicenseSeatBaseline> items, List<string> order) : ILicenseSeatBaselineStore
{
    public Dictionary<string, LicenseSeatBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatBaseline item)
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

    public List<LicenseSeatBaseline> List()
    {
        var outValue = new List<LicenseSeatBaseline>();
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
public sealed class CachedLicenseSeatBaselineStore(MemoryLicenseSeatBaselineStore inner, LicenseSeatBaselineCache cache, int hits, int misses) : ILicenseSeatBaselineStore
{
    public MemoryLicenseSeatBaselineStore Inner { get; set; } = inner;
    public LicenseSeatBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatBaseline? Find(string code)
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

    public void Save(LicenseSeatBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat baselines. Empty fields match all.</summary>
public sealed record LicenseSeatBaselineQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatBaseline> Apply(List<LicenseSeatBaseline> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat baseline store.</summary>
public static class LicenseSeatBaselineStore
{
    public static ILicenseSeatBaselineStore NewMemoryLicenseSeatBaselineStore()
    {
        return new MemoryLicenseSeatBaselineStore(new Dictionary<string, LicenseSeatBaseline>(), new List<string>());
    }

    public static ILicenseSeatBaselineStore NewCachedLicenseSeatBaselineStore(MemoryLicenseSeatBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatBaselineStore(inner, LicenseSeatBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatBaseline> FindLicenseSeatBaselines(ILicenseSeatBaselineStore store, LicenseSeatBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
