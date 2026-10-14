namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment quotas by code.</summary>
public interface IGarnishmentQuotaStore
{
    GarnishmentQuota? Find(string code);
    void Save(GarnishmentQuota item);
    bool Remove(string code);
    List<GarnishmentQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentQuotaStore(Dictionary<string, GarnishmentQuota> items, List<string> order) : IGarnishmentQuotaStore
{
    public Dictionary<string, GarnishmentQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentQuota item)
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

    public List<GarnishmentQuota> List()
    {
        var outValue = new List<GarnishmentQuota>();
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
public sealed class CachedGarnishmentQuotaStore(MemoryGarnishmentQuotaStore inner, GarnishmentQuotaCache cache, int hits, int misses) : IGarnishmentQuotaStore
{
    public MemoryGarnishmentQuotaStore Inner { get; set; } = inner;
    public GarnishmentQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentQuota? Find(string code)
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

    public void Save(GarnishmentQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment quotas. Empty fields match all.</summary>
public sealed record GarnishmentQuotaQuery(string Source, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<GarnishmentQuota> Apply(List<GarnishmentQuota> items)
    {
        var matched = Collections.Filter(items, (GarnishmentQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment quota store.</summary>
public static class GarnishmentQuotaStore
{
    public static IGarnishmentQuotaStore NewMemoryGarnishmentQuotaStore()
    {
        return new MemoryGarnishmentQuotaStore(new Dictionary<string, GarnishmentQuota>(), new List<string>());
    }

    public static IGarnishmentQuotaStore NewCachedGarnishmentQuotaStore(MemoryGarnishmentQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentQuotaStore(inner, GarnishmentQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentQuota> FindGarnishmentQuotas(IGarnishmentQuotaStore store, GarnishmentQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
