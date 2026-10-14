namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile metrics by code.</summary>
public interface ISkillProfileMetricStore
{
    SkillProfileMetric? Find(string code);
    void Save(SkillProfileMetric item);
    bool Remove(string code);
    List<SkillProfileMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileMetricStore(Dictionary<string, SkillProfileMetric> items, List<string> order) : ISkillProfileMetricStore
{
    public Dictionary<string, SkillProfileMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileMetric item)
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

    public List<SkillProfileMetric> List()
    {
        var outValue = new List<SkillProfileMetric>();
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
public sealed class CachedSkillProfileMetricStore(MemorySkillProfileMetricStore inner, SkillProfileMetricCache cache, int hits, int misses) : ISkillProfileMetricStore
{
    public MemorySkillProfileMetricStore Inner { get; set; } = inner;
    public SkillProfileMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileMetric? Find(string code)
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

    public void Save(SkillProfileMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile metrics. Empty fields match all.</summary>
public sealed record SkillProfileMetricQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileMetric item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileMetric> Apply(List<SkillProfileMetric> items)
    {
        var matched = Collections.Filter(items, (SkillProfileMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile metric store.</summary>
public static class SkillProfileMetricStore
{
    public static ISkillProfileMetricStore NewMemorySkillProfileMetricStore()
    {
        return new MemorySkillProfileMetricStore(new Dictionary<string, SkillProfileMetric>(), new List<string>());
    }

    public static ISkillProfileMetricStore NewCachedSkillProfileMetricStore(MemorySkillProfileMetricStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileMetricStore(inner, SkillProfileMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SkillProfileMetric> FindSkillProfileMetrics(ISkillProfileMetricStore store, SkillProfileMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
