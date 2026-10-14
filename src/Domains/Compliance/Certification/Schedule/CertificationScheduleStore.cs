namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification schedules by code.</summary>
public interface ICertificationScheduleStore
{
    CertificationSchedule? Find(string code);
    void Save(CertificationSchedule item);
    bool Remove(string code);
    List<CertificationSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationScheduleStore(Dictionary<string, CertificationSchedule> items, List<string> order) : ICertificationScheduleStore
{
    public Dictionary<string, CertificationSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationSchedule item)
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

    public List<CertificationSchedule> List()
    {
        var outValue = new List<CertificationSchedule>();
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
public sealed class CachedCertificationScheduleStore(MemoryCertificationScheduleStore inner, CertificationScheduleCache cache, int hits, int misses) : ICertificationScheduleStore
{
    public MemoryCertificationScheduleStore Inner { get; set; } = inner;
    public CertificationScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationSchedule? Find(string code)
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

    public void Save(CertificationSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification schedules. Empty fields match all.</summary>
public sealed record CertificationScheduleQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationSchedule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CertificationSchedule> Apply(List<CertificationSchedule> items)
    {
        var matched = Collections.Filter(items, (CertificationSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification schedule store.</summary>
public static class CertificationScheduleStore
{
    public static ICertificationScheduleStore NewMemoryCertificationScheduleStore()
    {
        return new MemoryCertificationScheduleStore(
            new Dictionary<string, CertificationSchedule>(),
            new List<string>());
    }

    public static ICertificationScheduleStore NewCachedCertificationScheduleStore(MemoryCertificationScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationScheduleStore(inner, CertificationScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CertificationSchedule> FindCertificationSchedules(ICertificationScheduleStore store, CertificationScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
