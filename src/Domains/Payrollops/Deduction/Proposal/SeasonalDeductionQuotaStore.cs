namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal deduction quotas by code.</summary>
public interface ISeasonalDeductionQuotaStore
{
    SeasonalDeductionQuota? Find(string code);
    void Save(SeasonalDeductionQuota item);
    bool Remove(string code);
    List<SeasonalDeductionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalDeductionQuotaStore(Dictionary<string, SeasonalDeductionQuota> items, List<string> order) : ISeasonalDeductionQuotaStore
{
    public Dictionary<string, SeasonalDeductionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalDeductionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalDeductionQuota item)
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

    public List<SeasonalDeductionQuota> List()
    {
        var outValue = new List<SeasonalDeductionQuota>();
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
public sealed class CachedSeasonalDeductionQuotaStore(MemorySeasonalDeductionQuotaStore inner, SeasonalDeductionQuotaCache cache, int hits, int misses) : ISeasonalDeductionQuotaStore
{
    public MemorySeasonalDeductionQuotaStore Inner { get; set; } = inner;
    public SeasonalDeductionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalDeductionQuota? Find(string code)
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

    public void Save(SeasonalDeductionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalDeductionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal deduction quotas. Empty fields match all.</summary>
public sealed record SeasonalDeductionQuotaQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalDeductionQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<SeasonalDeductionQuota> Apply(List<SeasonalDeductionQuota> items)
    {
        var matched = Collections.Filter(items, (SeasonalDeductionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal deduction quota store.</summary>
public static class SeasonalDeductionQuotaStore
{
    public static ISeasonalDeductionQuotaStore NewMemorySeasonalDeductionQuotaStore()
    {
        return new MemorySeasonalDeductionQuotaStore(
            new Dictionary<string, SeasonalDeductionQuota>(),
            new List<string>());
    }

    public static ISeasonalDeductionQuotaStore NewCachedSeasonalDeductionQuotaStore(MemorySeasonalDeductionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalDeductionQuotaStore(
            inner,
            SeasonalDeductionQuotaCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalDeductionQuota> FindSeasonalDeductionQuotas(ISeasonalDeductionQuotaStore store, SeasonalDeductionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
