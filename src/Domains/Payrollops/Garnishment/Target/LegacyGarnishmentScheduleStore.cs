namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy garnishment schedules by code.</summary>
public interface ILegacyGarnishmentScheduleStore
{
    LegacyGarnishmentSchedule? Find(string code);
    void Save(LegacyGarnishmentSchedule item);
    bool Remove(string code);
    List<LegacyGarnishmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyGarnishmentScheduleStore(Dictionary<string, LegacyGarnishmentSchedule> items, List<string> order) : ILegacyGarnishmentScheduleStore
{
    public Dictionary<string, LegacyGarnishmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyGarnishmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyGarnishmentSchedule item)
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

    public List<LegacyGarnishmentSchedule> List()
    {
        var outValue = new List<LegacyGarnishmentSchedule>();
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
public sealed class CachedLegacyGarnishmentScheduleStore(MemoryLegacyGarnishmentScheduleStore inner, LegacyGarnishmentScheduleCache cache, int hits, int misses) : ILegacyGarnishmentScheduleStore
{
    public MemoryLegacyGarnishmentScheduleStore Inner { get; set; } = inner;
    public LegacyGarnishmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyGarnishmentSchedule? Find(string code)
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

    public void Save(LegacyGarnishmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyGarnishmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy garnishment schedules. Empty fields match all.</summary>
public sealed record LegacyGarnishmentScheduleQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyGarnishmentSchedule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyGarnishmentSchedule> Apply(List<LegacyGarnishmentSchedule> items)
    {
        var matched = Collections.Filter(items, (LegacyGarnishmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy garnishment schedule store.</summary>
public static class LegacyGarnishmentScheduleStore
{
    public static ILegacyGarnishmentScheduleStore NewMemoryLegacyGarnishmentScheduleStore()
    {
        return new MemoryLegacyGarnishmentScheduleStore(
            new Dictionary<string, LegacyGarnishmentSchedule>(),
            new List<string>());
    }

    public static ILegacyGarnishmentScheduleStore NewCachedLegacyGarnishmentScheduleStore(MemoryLegacyGarnishmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyGarnishmentScheduleStore(
            inner,
            LegacyGarnishmentScheduleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyGarnishmentSchedule> FindLegacyGarnishmentSchedules(ILegacyGarnishmentScheduleStore store, LegacyGarnishmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
