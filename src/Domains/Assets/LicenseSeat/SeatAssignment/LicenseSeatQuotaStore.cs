namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat quotas by code.</summary>
public interface ILicenseSeatQuotaStore
{
    LicenseSeatQuota? Find(string code);
    void Save(LicenseSeatQuota item);
    bool Remove(string code);
    List<LicenseSeatQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatQuotaStore(Dictionary<string, LicenseSeatQuota> items, List<string> order) : ILicenseSeatQuotaStore
{
    public Dictionary<string, LicenseSeatQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatQuota item)
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

    public List<LicenseSeatQuota> List()
    {
        var outValue = new List<LicenseSeatQuota>();
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
public sealed class CachedLicenseSeatQuotaStore(MemoryLicenseSeatQuotaStore inner, LicenseSeatQuotaCache cache, int hits, int misses) : ILicenseSeatQuotaStore
{
    public MemoryLicenseSeatQuotaStore Inner { get; set; } = inner;
    public LicenseSeatQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatQuota? Find(string code)
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

    public void Save(LicenseSeatQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat quotas. Empty fields match all.</summary>
public sealed record LicenseSeatQuotaQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatQuota item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LicenseSeatQuota> Apply(List<LicenseSeatQuota> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat quota store.</summary>
public static class LicenseSeatQuotaStore
{
    public static ILicenseSeatQuotaStore NewMemoryLicenseSeatQuotaStore()
    {
        return new MemoryLicenseSeatQuotaStore(new Dictionary<string, LicenseSeatQuota>(), new List<string>());
    }

    public static ILicenseSeatQuotaStore NewCachedLicenseSeatQuotaStore(MemoryLicenseSeatQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatQuotaStore(inner, LicenseSeatQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatQuota> FindLicenseSeatQuotas(ILicenseSeatQuotaStore store, LicenseSeatQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
