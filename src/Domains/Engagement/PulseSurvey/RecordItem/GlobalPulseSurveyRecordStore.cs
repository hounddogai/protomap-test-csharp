namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pulse survey records by code.</summary>
public interface IGlobalPulseSurveyRecordStore
{
    GlobalPulseSurveyRecord? Find(string code);
    void Save(GlobalPulseSurveyRecord item);
    bool Remove(string code);
    List<GlobalPulseSurveyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPulseSurveyRecordStore(Dictionary<string, GlobalPulseSurveyRecord> items, List<string> order) : IGlobalPulseSurveyRecordStore
{
    public Dictionary<string, GlobalPulseSurveyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPulseSurveyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPulseSurveyRecord item)
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

    public List<GlobalPulseSurveyRecord> List()
    {
        var outValue = new List<GlobalPulseSurveyRecord>();
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
public sealed class CachedGlobalPulseSurveyRecordStore(MemoryGlobalPulseSurveyRecordStore inner, GlobalPulseSurveyRecordCache cache, int hits, int misses) : IGlobalPulseSurveyRecordStore
{
    public MemoryGlobalPulseSurveyRecordStore Inner { get; set; } = inner;
    public GlobalPulseSurveyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPulseSurveyRecord? Find(string code)
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

    public void Save(GlobalPulseSurveyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPulseSurveyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pulse survey records. Empty fields match all.</summary>
public sealed record GlobalPulseSurveyRecordQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPulseSurveyRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPulseSurveyRecord> Apply(List<GlobalPulseSurveyRecord> items)
    {
        var matched = Collections.Filter(items, (GlobalPulseSurveyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pulse survey record store.</summary>
public static class GlobalPulseSurveyRecordStore
{
    public static IGlobalPulseSurveyRecordStore NewMemoryGlobalPulseSurveyRecordStore()
    {
        return new MemoryGlobalPulseSurveyRecordStore(
            new Dictionary<string, GlobalPulseSurveyRecord>(),
            new List<string>());
    }

    public static IGlobalPulseSurveyRecordStore NewCachedGlobalPulseSurveyRecordStore(MemoryGlobalPulseSurveyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPulseSurveyRecordStore(
            inner,
            GlobalPulseSurveyRecordCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPulseSurveyRecord> FindGlobalPulseSurveyRecords(IGlobalPulseSurveyRecordStore store, GlobalPulseSurveyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
