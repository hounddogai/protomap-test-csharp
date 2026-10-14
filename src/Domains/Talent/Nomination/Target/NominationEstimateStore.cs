namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination estimates by code.</summary>
public interface INominationEstimateStore
{
    NominationEstimate? Find(string code);
    void Save(NominationEstimate item);
    bool Remove(string code);
    List<NominationEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationEstimateStore(Dictionary<string, NominationEstimate> items, List<string> order) : INominationEstimateStore
{
    public Dictionary<string, NominationEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationEstimate item)
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

    public List<NominationEstimate> List()
    {
        var outValue = new List<NominationEstimate>();
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
public sealed class CachedNominationEstimateStore(MemoryNominationEstimateStore inner, NominationEstimateCache cache, int hits, int misses) : INominationEstimateStore
{
    public MemoryNominationEstimateStore Inner { get; set; } = inner;
    public NominationEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationEstimate? Find(string code)
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

    public void Save(NominationEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination estimates. Empty fields match all.</summary>
public sealed record NominationEstimateQuery(string Department, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(NominationEstimate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<NominationEstimate> Apply(List<NominationEstimate> items)
    {
        var matched = Collections.Filter(items, (NominationEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination estimate store.</summary>
public static class NominationEstimateStore
{
    public static INominationEstimateStore NewMemoryNominationEstimateStore()
    {
        return new MemoryNominationEstimateStore(new Dictionary<string, NominationEstimate>(), new List<string>());
    }

    public static INominationEstimateStore NewCachedNominationEstimateStore(MemoryNominationEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedNominationEstimateStore(inner, NominationEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationEstimate> FindNominationEstimates(INominationEstimateStore store, NominationEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
