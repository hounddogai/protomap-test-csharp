namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional attestation schedules by code.</summary>
public interface IRegionalAttestationScheduleStore
{
    RegionalAttestationSchedule? Find(string code);
    void Save(RegionalAttestationSchedule item);
    bool Remove(string code);
    List<RegionalAttestationSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAttestationScheduleStore(Dictionary<string, RegionalAttestationSchedule> items, List<string> order) : IRegionalAttestationScheduleStore
{
    public Dictionary<string, RegionalAttestationSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAttestationSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAttestationSchedule item)
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

    public List<RegionalAttestationSchedule> List()
    {
        var outValue = new List<RegionalAttestationSchedule>();
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
public sealed class CachedRegionalAttestationScheduleStore(MemoryRegionalAttestationScheduleStore inner, RegionalAttestationScheduleCache cache, int hits, int misses) : IRegionalAttestationScheduleStore
{
    public MemoryRegionalAttestationScheduleStore Inner { get; set; } = inner;
    public RegionalAttestationScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAttestationSchedule? Find(string code)
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

    public void Save(RegionalAttestationSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAttestationSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional attestation schedules. Empty fields match all.</summary>
public sealed record RegionalAttestationScheduleQuery(string Department, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAttestationSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAttestationSchedule> Apply(List<RegionalAttestationSchedule> items)
    {
        var matched = Collections.Filter(items, (RegionalAttestationSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional attestation schedule store.</summary>
public static class RegionalAttestationScheduleStore
{
    public static IRegionalAttestationScheduleStore NewMemoryRegionalAttestationScheduleStore()
    {
        return new MemoryRegionalAttestationScheduleStore(
            new Dictionary<string, RegionalAttestationSchedule>(),
            new List<string>());
    }

    public static IRegionalAttestationScheduleStore NewCachedRegionalAttestationScheduleStore(MemoryRegionalAttestationScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAttestationScheduleStore(
            inner,
            RegionalAttestationScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAttestationSchedule> FindRegionalAttestationSchedules(IRegionalAttestationScheduleStore store, RegionalAttestationScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
