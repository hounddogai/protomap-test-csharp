namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool schedules by code.</summary>
public interface IAdaptiveBonusPoolScheduleStore
{
    AdaptiveBonusPoolSchedule? Find(string code);
    void Save(AdaptiveBonusPoolSchedule item);
    bool Remove(string code);
    List<AdaptiveBonusPoolSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolScheduleStore(Dictionary<string, AdaptiveBonusPoolSchedule> items, List<string> order) : IAdaptiveBonusPoolScheduleStore
{
    public Dictionary<string, AdaptiveBonusPoolSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolSchedule item)
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

    public List<AdaptiveBonusPoolSchedule> List()
    {
        var outValue = new List<AdaptiveBonusPoolSchedule>();
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
public sealed class CachedAdaptiveBonusPoolScheduleStore(MemoryAdaptiveBonusPoolScheduleStore inner, AdaptiveBonusPoolScheduleCache cache, int hits, int misses) : IAdaptiveBonusPoolScheduleStore
{
    public MemoryAdaptiveBonusPoolScheduleStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolSchedule? Find(string code)
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

    public void Save(AdaptiveBonusPoolSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool schedules. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolScheduleQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolSchedule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBonusPoolSchedule> Apply(List<AdaptiveBonusPoolSchedule> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool schedule store.</summary>
public static class AdaptiveBonusPoolScheduleStore
{
    public static IAdaptiveBonusPoolScheduleStore NewMemoryAdaptiveBonusPoolScheduleStore()
    {
        return new MemoryAdaptiveBonusPoolScheduleStore(
            new Dictionary<string, AdaptiveBonusPoolSchedule>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolScheduleStore NewCachedAdaptiveBonusPoolScheduleStore(MemoryAdaptiveBonusPoolScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolScheduleStore(
            inner,
            AdaptiveBonusPoolScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolSchedule> FindAdaptiveBonusPoolSchedules(IAdaptiveBonusPoolScheduleStore store, AdaptiveBonusPoolScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
