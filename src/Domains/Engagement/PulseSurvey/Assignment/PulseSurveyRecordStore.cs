namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey records by code.</summary>
public interface IPulseSurveyRecordStore
{
    PulseSurveyRecord? Find(string code);
    void Save(PulseSurveyRecord item);
    bool Remove(string code);
    List<PulseSurveyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyRecordStore(Dictionary<string, PulseSurveyRecord> items, List<string> order) : IPulseSurveyRecordStore
{
    public Dictionary<string, PulseSurveyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyRecord item)
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

    public List<PulseSurveyRecord> List()
    {
        var outValue = new List<PulseSurveyRecord>();
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
public sealed class CachedPulseSurveyRecordStore(MemoryPulseSurveyRecordStore inner, PulseSurveyRecordCache cache, int hits, int misses) : IPulseSurveyRecordStore
{
    public MemoryPulseSurveyRecordStore Inner { get; set; } = inner;
    public PulseSurveyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyRecord? Find(string code)
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

    public void Save(PulseSurveyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey records. Empty fields match all.</summary>
public sealed record PulseSurveyRecordQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyRecord item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PulseSurveyRecord> Apply(List<PulseSurveyRecord> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey record store.</summary>
public static class PulseSurveyRecordStore
{
    public static IPulseSurveyRecordStore NewMemoryPulseSurveyRecordStore()
    {
        return new MemoryPulseSurveyRecordStore(new Dictionary<string, PulseSurveyRecord>(), new List<string>());
    }

    public static IPulseSurveyRecordStore NewCachedPulseSurveyRecordStore(MemoryPulseSurveyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyRecordStore(inner, PulseSurveyRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyRecord> FindPulseSurveyRecords(IPulseSurveyRecordStore store, PulseSurveyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
