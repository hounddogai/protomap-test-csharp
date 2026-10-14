namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment schedules by code.</summary>
public interface IGarnishmentScheduleStore
{
    GarnishmentSchedule? Find(string code);
    void Save(GarnishmentSchedule item);
    bool Remove(string code);
    List<GarnishmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentScheduleStore(Dictionary<string, GarnishmentSchedule> items, List<string> order) : IGarnishmentScheduleStore
{
    public Dictionary<string, GarnishmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentSchedule item)
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

    public List<GarnishmentSchedule> List()
    {
        var outValue = new List<GarnishmentSchedule>();
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
public sealed class CachedGarnishmentScheduleStore(MemoryGarnishmentScheduleStore inner, GarnishmentScheduleCache cache, int hits, int misses) : IGarnishmentScheduleStore
{
    public MemoryGarnishmentScheduleStore Inner { get; set; } = inner;
    public GarnishmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentSchedule? Find(string code)
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

    public void Save(GarnishmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment schedules. Empty fields match all.</summary>
public sealed record GarnishmentScheduleQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentSchedule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentSchedule> Apply(List<GarnishmentSchedule> items)
    {
        var matched = Collections.Filter(items, (GarnishmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment schedule store.</summary>
public static class GarnishmentScheduleStore
{
    public static IGarnishmentScheduleStore NewMemoryGarnishmentScheduleStore()
    {
        return new MemoryGarnishmentScheduleStore(new Dictionary<string, GarnishmentSchedule>(), new List<string>());
    }

    public static IGarnishmentScheduleStore NewCachedGarnishmentScheduleStore(MemoryGarnishmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentScheduleStore(inner, GarnishmentScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GarnishmentSchedule> FindGarnishmentSchedules(IGarnishmentScheduleStore store, GarnishmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
