namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled feedback records by code.</summary>
public interface IScheduledFeedbackRecordStore
{
    ScheduledFeedbackRecord? Find(string code);
    void Save(ScheduledFeedbackRecord item);
    bool Remove(string code);
    List<ScheduledFeedbackRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFeedbackRecordStore(Dictionary<string, ScheduledFeedbackRecord> items, List<string> order) : IScheduledFeedbackRecordStore
{
    public Dictionary<string, ScheduledFeedbackRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFeedbackRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFeedbackRecord item)
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

    public List<ScheduledFeedbackRecord> List()
    {
        var outValue = new List<ScheduledFeedbackRecord>();
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
public sealed class CachedScheduledFeedbackRecordStore(MemoryScheduledFeedbackRecordStore inner, ScheduledFeedbackRecordCache cache, int hits, int misses) : IScheduledFeedbackRecordStore
{
    public MemoryScheduledFeedbackRecordStore Inner { get; set; } = inner;
    public ScheduledFeedbackRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFeedbackRecord? Find(string code)
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

    public void Save(ScheduledFeedbackRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFeedbackRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled feedback records. Empty fields match all.</summary>
public sealed record ScheduledFeedbackRecordQuery(string Segment, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFeedbackRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFeedbackRecord> Apply(List<ScheduledFeedbackRecord> items)
    {
        var matched = Collections.Filter(items, (ScheduledFeedbackRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled feedback record store.</summary>
public static class ScheduledFeedbackRecordStore
{
    public static IScheduledFeedbackRecordStore NewMemoryScheduledFeedbackRecordStore()
    {
        return new MemoryScheduledFeedbackRecordStore(
            new Dictionary<string, ScheduledFeedbackRecord>(),
            new List<string>());
    }

    public static IScheduledFeedbackRecordStore NewCachedScheduledFeedbackRecordStore(MemoryScheduledFeedbackRecordStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFeedbackRecordStore(
            inner,
            ScheduledFeedbackRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFeedbackRecord> FindScheduledFeedbackRecords(IScheduledFeedbackRecordStore store, ScheduledFeedbackRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
