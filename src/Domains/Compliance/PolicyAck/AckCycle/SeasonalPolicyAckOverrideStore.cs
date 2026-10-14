namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal policy ack overrides by code.</summary>
public interface ISeasonalPolicyAckOverrideStore
{
    SeasonalPolicyAckOverride? Find(string code);
    void Save(SeasonalPolicyAckOverride item);
    bool Remove(string code);
    List<SeasonalPolicyAckOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPolicyAckOverrideStore(Dictionary<string, SeasonalPolicyAckOverride> items, List<string> order) : ISeasonalPolicyAckOverrideStore
{
    public Dictionary<string, SeasonalPolicyAckOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPolicyAckOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPolicyAckOverride item)
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

    public List<SeasonalPolicyAckOverride> List()
    {
        var outValue = new List<SeasonalPolicyAckOverride>();
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
public sealed class CachedSeasonalPolicyAckOverrideStore(MemorySeasonalPolicyAckOverrideStore inner, SeasonalPolicyAckOverrideCache cache, int hits, int misses) : ISeasonalPolicyAckOverrideStore
{
    public MemorySeasonalPolicyAckOverrideStore Inner { get; set; } = inner;
    public SeasonalPolicyAckOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPolicyAckOverride? Find(string code)
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

    public void Save(SeasonalPolicyAckOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPolicyAckOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal policy ack overrides. Empty fields match all.</summary>
public sealed record SeasonalPolicyAckOverrideQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPolicyAckOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPolicyAckOverride> Apply(List<SeasonalPolicyAckOverride> items)
    {
        var matched = Collections.Filter(items, (SeasonalPolicyAckOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal policy ack override store.</summary>
public static class SeasonalPolicyAckOverrideStore
{
    public static ISeasonalPolicyAckOverrideStore NewMemorySeasonalPolicyAckOverrideStore()
    {
        return new MemorySeasonalPolicyAckOverrideStore(
            new Dictionary<string, SeasonalPolicyAckOverride>(),
            new List<string>());
    }

    public static ISeasonalPolicyAckOverrideStore NewCachedSeasonalPolicyAckOverrideStore(MemorySeasonalPolicyAckOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPolicyAckOverrideStore(
            inner,
            SeasonalPolicyAckOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPolicyAckOverride> FindSeasonalPolicyAckOverrides(ISeasonalPolicyAckOverrideStore store, SeasonalPolicyAckOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
