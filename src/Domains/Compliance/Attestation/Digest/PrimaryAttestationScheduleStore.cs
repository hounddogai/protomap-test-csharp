namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary attestation schedules by code.</summary>
public interface IPrimaryAttestationScheduleStore
{
    PrimaryAttestationSchedule? Find(string code);
    void Save(PrimaryAttestationSchedule item);
    bool Remove(string code);
    List<PrimaryAttestationSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAttestationScheduleStore(Dictionary<string, PrimaryAttestationSchedule> items, List<string> order) : IPrimaryAttestationScheduleStore
{
    public Dictionary<string, PrimaryAttestationSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAttestationSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAttestationSchedule item)
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

    public List<PrimaryAttestationSchedule> List()
    {
        var outValue = new List<PrimaryAttestationSchedule>();
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
public sealed class CachedPrimaryAttestationScheduleStore(MemoryPrimaryAttestationScheduleStore inner, PrimaryAttestationScheduleCache cache, int hits, int misses) : IPrimaryAttestationScheduleStore
{
    public MemoryPrimaryAttestationScheduleStore Inner { get; set; } = inner;
    public PrimaryAttestationScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAttestationSchedule? Find(string code)
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

    public void Save(PrimaryAttestationSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAttestationSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary attestation schedules. Empty fields match all.</summary>
public sealed record PrimaryAttestationScheduleQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAttestationSchedule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAttestationSchedule> Apply(List<PrimaryAttestationSchedule> items)
    {
        var matched = Collections.Filter(items, (PrimaryAttestationSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary attestation schedule store.</summary>
public static class PrimaryAttestationScheduleStore
{
    public static IPrimaryAttestationScheduleStore NewMemoryPrimaryAttestationScheduleStore()
    {
        return new MemoryPrimaryAttestationScheduleStore(
            new Dictionary<string, PrimaryAttestationSchedule>(),
            new List<string>());
    }

    public static IPrimaryAttestationScheduleStore NewCachedPrimaryAttestationScheduleStore(MemoryPrimaryAttestationScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAttestationScheduleStore(
            inner,
            PrimaryAttestationScheduleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAttestationSchedule> FindPrimaryAttestationSchedules(IPrimaryAttestationScheduleStore store, PrimaryAttestationScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
