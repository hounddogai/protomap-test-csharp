namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination variances by code.</summary>
public interface INominationVarianceStore
{
    NominationVariance? Find(string code);
    void Save(NominationVariance item);
    bool Remove(string code);
    List<NominationVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationVarianceStore(Dictionary<string, NominationVariance> items, List<string> order) : INominationVarianceStore
{
    public Dictionary<string, NominationVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationVariance item)
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

    public List<NominationVariance> List()
    {
        var outValue = new List<NominationVariance>();
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
public sealed class CachedNominationVarianceStore(MemoryNominationVarianceStore inner, NominationVarianceCache cache, int hits, int misses) : INominationVarianceStore
{
    public MemoryNominationVarianceStore Inner { get; set; } = inner;
    public NominationVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationVariance? Find(string code)
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

    public void Save(NominationVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination variances. Empty fields match all.</summary>
public sealed record NominationVarianceQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(NominationVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationVariance> Apply(List<NominationVariance> items)
    {
        var matched = Collections.Filter(items, (NominationVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination variance store.</summary>
public static class NominationVarianceStore
{
    public static INominationVarianceStore NewMemoryNominationVarianceStore()
    {
        return new MemoryNominationVarianceStore(new Dictionary<string, NominationVariance>(), new List<string>());
    }

    public static INominationVarianceStore NewCachedNominationVarianceStore(MemoryNominationVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedNominationVarianceStore(inner, NominationVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<NominationVariance> FindNominationVariances(INominationVarianceStore store, NominationVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
