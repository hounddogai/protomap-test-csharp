namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered engagement agreements by code.</summary>
public interface ITieredEngagementAgreementStore
{
    TieredEngagementAgreement? Find(string code);
    void Save(TieredEngagementAgreement item);
    bool Remove(string code);
    List<TieredEngagementAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredEngagementAgreementStore(Dictionary<string, TieredEngagementAgreement> items, List<string> order) : ITieredEngagementAgreementStore
{
    public Dictionary<string, TieredEngagementAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredEngagementAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredEngagementAgreement item)
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

    public List<TieredEngagementAgreement> List()
    {
        var outValue = new List<TieredEngagementAgreement>();
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
public sealed class CachedTieredEngagementAgreementStore(MemoryTieredEngagementAgreementStore inner, TieredEngagementAgreementCache cache, int hits, int misses) : ITieredEngagementAgreementStore
{
    public MemoryTieredEngagementAgreementStore Inner { get; set; } = inner;
    public TieredEngagementAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredEngagementAgreement? Find(string code)
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

    public void Save(TieredEngagementAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredEngagementAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered engagement agreements. Empty fields match all.</summary>
public sealed record TieredEngagementAgreementQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TieredEngagementAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredEngagementAgreement> Apply(List<TieredEngagementAgreement> items)
    {
        var matched = Collections.Filter(items, (TieredEngagementAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered engagement agreement store.</summary>
public static class TieredEngagementAgreementStore
{
    public static ITieredEngagementAgreementStore NewMemoryTieredEngagementAgreementStore()
    {
        return new MemoryTieredEngagementAgreementStore(
            new Dictionary<string, TieredEngagementAgreement>(),
            new List<string>());
    }

    public static ITieredEngagementAgreementStore NewCachedTieredEngagementAgreementStore(MemoryTieredEngagementAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTieredEngagementAgreementStore(
            inner,
            TieredEngagementAgreementCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredEngagementAgreement> FindTieredEngagementAgreements(ITieredEngagementAgreementStore store, TieredEngagementAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
