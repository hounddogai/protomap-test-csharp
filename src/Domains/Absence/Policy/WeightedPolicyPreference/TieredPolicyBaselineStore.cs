namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered policy baselines by code.</summary>
public interface ITieredPolicyBaselineStore
{
    TieredPolicyBaseline? Find(string code);
    void Save(TieredPolicyBaseline item);
    bool Remove(string code);
    List<TieredPolicyBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPolicyBaselineStore(Dictionary<string, TieredPolicyBaseline> items, List<string> order) : ITieredPolicyBaselineStore
{
    public Dictionary<string, TieredPolicyBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPolicyBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPolicyBaseline item)
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

    public List<TieredPolicyBaseline> List()
    {
        var outValue = new List<TieredPolicyBaseline>();
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
public sealed class CachedTieredPolicyBaselineStore(MemoryTieredPolicyBaselineStore inner, TieredPolicyBaselineCache cache, int hits, int misses) : ITieredPolicyBaselineStore
{
    public MemoryTieredPolicyBaselineStore Inner { get; set; } = inner;
    public TieredPolicyBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPolicyBaseline? Find(string code)
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

    public void Save(TieredPolicyBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPolicyBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered policy baselines. Empty fields match all.</summary>
public sealed record TieredPolicyBaselineQuery(string Team, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPolicyBaseline item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPolicyBaseline> Apply(List<TieredPolicyBaseline> items)
    {
        var matched = Collections.Filter(items, (TieredPolicyBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered policy baseline store.</summary>
public static class TieredPolicyBaselineStore
{
    public static ITieredPolicyBaselineStore NewMemoryTieredPolicyBaselineStore()
    {
        return new MemoryTieredPolicyBaselineStore(
            new Dictionary<string, TieredPolicyBaseline>(),
            new List<string>());
    }

    public static ITieredPolicyBaselineStore NewCachedTieredPolicyBaselineStore(MemoryTieredPolicyBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPolicyBaselineStore(inner, TieredPolicyBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredPolicyBaseline> FindTieredPolicyBaselines(ITieredPolicyBaselineStore store, TieredPolicyBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
