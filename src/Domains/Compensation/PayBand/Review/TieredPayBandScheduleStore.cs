namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pay band schedules by code.</summary>
public interface ITieredPayBandScheduleStore
{
    TieredPayBandSchedule? Find(string code);
    void Save(TieredPayBandSchedule item);
    bool Remove(string code);
    List<TieredPayBandSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPayBandScheduleStore(Dictionary<string, TieredPayBandSchedule> items, List<string> order) : ITieredPayBandScheduleStore
{
    public Dictionary<string, TieredPayBandSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPayBandSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPayBandSchedule item)
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

    public List<TieredPayBandSchedule> List()
    {
        var outValue = new List<TieredPayBandSchedule>();
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
public sealed class CachedTieredPayBandScheduleStore(MemoryTieredPayBandScheduleStore inner, TieredPayBandScheduleCache cache, int hits, int misses) : ITieredPayBandScheduleStore
{
    public MemoryTieredPayBandScheduleStore Inner { get; set; } = inner;
    public TieredPayBandScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPayBandSchedule? Find(string code)
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

    public void Save(TieredPayBandSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPayBandSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pay band schedules. Empty fields match all.</summary>
public sealed record TieredPayBandScheduleQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPayBandSchedule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPayBandSchedule> Apply(List<TieredPayBandSchedule> items)
    {
        var matched = Collections.Filter(items, (TieredPayBandSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pay band schedule store.</summary>
public static class TieredPayBandScheduleStore
{
    public static ITieredPayBandScheduleStore NewMemoryTieredPayBandScheduleStore()
    {
        return new MemoryTieredPayBandScheduleStore(
            new Dictionary<string, TieredPayBandSchedule>(),
            new List<string>());
    }

    public static ITieredPayBandScheduleStore NewCachedTieredPayBandScheduleStore(MemoryTieredPayBandScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPayBandScheduleStore(inner, TieredPayBandScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredPayBandSchedule> FindTieredPayBandSchedules(ITieredPayBandScheduleStore store, TieredPayBandScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
