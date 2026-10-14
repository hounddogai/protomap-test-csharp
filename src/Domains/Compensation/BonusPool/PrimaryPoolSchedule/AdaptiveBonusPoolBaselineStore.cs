namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool baselines by code.</summary>
public interface IAdaptiveBonusPoolBaselineStore
{
    AdaptiveBonusPoolBaseline? Find(string code);
    void Save(AdaptiveBonusPoolBaseline item);
    bool Remove(string code);
    List<AdaptiveBonusPoolBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolBaselineStore(Dictionary<string, AdaptiveBonusPoolBaseline> items, List<string> order) : IAdaptiveBonusPoolBaselineStore
{
    public Dictionary<string, AdaptiveBonusPoolBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolBaseline item)
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

    public List<AdaptiveBonusPoolBaseline> List()
    {
        var outValue = new List<AdaptiveBonusPoolBaseline>();
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
public sealed class CachedAdaptiveBonusPoolBaselineStore(MemoryAdaptiveBonusPoolBaselineStore inner, AdaptiveBonusPoolBaselineCache cache, int hits, int misses) : IAdaptiveBonusPoolBaselineStore
{
    public MemoryAdaptiveBonusPoolBaselineStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolBaseline? Find(string code)
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

    public void Save(AdaptiveBonusPoolBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool baselines. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolBaselineQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AdaptiveBonusPoolBaseline> Apply(List<AdaptiveBonusPoolBaseline> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool baseline store.</summary>
public static class AdaptiveBonusPoolBaselineStore
{
    public static IAdaptiveBonusPoolBaselineStore NewMemoryAdaptiveBonusPoolBaselineStore()
    {
        return new MemoryAdaptiveBonusPoolBaselineStore(
            new Dictionary<string, AdaptiveBonusPoolBaseline>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolBaselineStore NewCachedAdaptiveBonusPoolBaselineStore(MemoryAdaptiveBonusPoolBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolBaselineStore(
            inner,
            AdaptiveBonusPoolBaselineCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolBaseline> FindAdaptiveBonusPoolBaselines(IAdaptiveBonusPoolBaselineStore store, AdaptiveBonusPoolBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
