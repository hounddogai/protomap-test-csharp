namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile estimates by code.</summary>
public interface ISkillProfileEstimateStore
{
    SkillProfileEstimate? Find(string code);
    void Save(SkillProfileEstimate item);
    bool Remove(string code);
    List<SkillProfileEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileEstimateStore(Dictionary<string, SkillProfileEstimate> items, List<string> order) : ISkillProfileEstimateStore
{
    public Dictionary<string, SkillProfileEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileEstimate item)
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

    public List<SkillProfileEstimate> List()
    {
        var outValue = new List<SkillProfileEstimate>();
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
public sealed class CachedSkillProfileEstimateStore(MemorySkillProfileEstimateStore inner, SkillProfileEstimateCache cache, int hits, int misses) : ISkillProfileEstimateStore
{
    public MemorySkillProfileEstimateStore Inner { get; set; } = inner;
    public SkillProfileEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileEstimate? Find(string code)
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

    public void Save(SkillProfileEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile estimates. Empty fields match all.</summary>
public sealed record SkillProfileEstimateQuery(string Locale, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileEstimate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileEstimate> Apply(List<SkillProfileEstimate> items)
    {
        var matched = Collections.Filter(items, (SkillProfileEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile estimate store.</summary>
public static class SkillProfileEstimateStore
{
    public static ISkillProfileEstimateStore NewMemorySkillProfileEstimateStore()
    {
        return new MemorySkillProfileEstimateStore(
            new Dictionary<string, SkillProfileEstimate>(),
            new List<string>());
    }

    public static ISkillProfileEstimateStore NewCachedSkillProfileEstimateStore(MemorySkillProfileEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileEstimateStore(inner, SkillProfileEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SkillProfileEstimate> FindSkillProfileEstimates(ISkillProfileEstimateStore store, SkillProfileEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
