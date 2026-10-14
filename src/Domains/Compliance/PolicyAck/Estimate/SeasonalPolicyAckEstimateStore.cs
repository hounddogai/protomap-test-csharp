namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal policy ack estimates by code.</summary>
public interface ISeasonalPolicyAckEstimateStore
{
    SeasonalPolicyAckEstimate? Find(string code);
    void Save(SeasonalPolicyAckEstimate item);
    bool Remove(string code);
    List<SeasonalPolicyAckEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPolicyAckEstimateStore(Dictionary<string, SeasonalPolicyAckEstimate> items, List<string> order) : ISeasonalPolicyAckEstimateStore
{
    public Dictionary<string, SeasonalPolicyAckEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPolicyAckEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPolicyAckEstimate item)
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

    public List<SeasonalPolicyAckEstimate> List()
    {
        var outValue = new List<SeasonalPolicyAckEstimate>();
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
public sealed class CachedSeasonalPolicyAckEstimateStore(MemorySeasonalPolicyAckEstimateStore inner, SeasonalPolicyAckEstimateCache cache, int hits, int misses) : ISeasonalPolicyAckEstimateStore
{
    public MemorySeasonalPolicyAckEstimateStore Inner { get; set; } = inner;
    public SeasonalPolicyAckEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPolicyAckEstimate? Find(string code)
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

    public void Save(SeasonalPolicyAckEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPolicyAckEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal policy ack estimates. Empty fields match all.</summary>
public sealed record SeasonalPolicyAckEstimateQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPolicyAckEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<SeasonalPolicyAckEstimate> Apply(List<SeasonalPolicyAckEstimate> items)
    {
        var matched = Collections.Filter(items, (SeasonalPolicyAckEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal policy ack estimate store.</summary>
public static class SeasonalPolicyAckEstimateStore
{
    public static ISeasonalPolicyAckEstimateStore NewMemorySeasonalPolicyAckEstimateStore()
    {
        return new MemorySeasonalPolicyAckEstimateStore(
            new Dictionary<string, SeasonalPolicyAckEstimate>(),
            new List<string>());
    }

    public static ISeasonalPolicyAckEstimateStore NewCachedSeasonalPolicyAckEstimateStore(MemorySeasonalPolicyAckEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPolicyAckEstimateStore(
            inner,
            SeasonalPolicyAckEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPolicyAckEstimate> FindSeasonalPolicyAckEstimates(ISeasonalPolicyAckEstimateStore store, SeasonalPolicyAckEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
