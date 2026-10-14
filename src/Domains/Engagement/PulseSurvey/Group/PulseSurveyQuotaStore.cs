namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey quotas by code.</summary>
public interface IPulseSurveyQuotaStore
{
    PulseSurveyQuota? Find(string code);
    void Save(PulseSurveyQuota item);
    bool Remove(string code);
    List<PulseSurveyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyQuotaStore(Dictionary<string, PulseSurveyQuota> items, List<string> order) : IPulseSurveyQuotaStore
{
    public Dictionary<string, PulseSurveyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyQuota item)
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

    public List<PulseSurveyQuota> List()
    {
        var outValue = new List<PulseSurveyQuota>();
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
public sealed class CachedPulseSurveyQuotaStore(MemoryPulseSurveyQuotaStore inner, PulseSurveyQuotaCache cache, int hits, int misses) : IPulseSurveyQuotaStore
{
    public MemoryPulseSurveyQuotaStore Inner { get; set; } = inner;
    public PulseSurveyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyQuota? Find(string code)
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

    public void Save(PulseSurveyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey quotas. Empty fields match all.</summary>
public sealed record PulseSurveyQuotaQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PulseSurveyQuota> Apply(List<PulseSurveyQuota> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey quota store.</summary>
public static class PulseSurveyQuotaStore
{
    public static IPulseSurveyQuotaStore NewMemoryPulseSurveyQuotaStore()
    {
        return new MemoryPulseSurveyQuotaStore(new Dictionary<string, PulseSurveyQuota>(), new List<string>());
    }

    public static IPulseSurveyQuotaStore NewCachedPulseSurveyQuotaStore(MemoryPulseSurveyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyQuotaStore(inner, PulseSurveyQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyQuota> FindPulseSurveyQuotas(IPulseSurveyQuotaStore store, PulseSurveyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
