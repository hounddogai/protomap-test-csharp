namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled exception drafts by code.</summary>
public interface IScheduledExceptionDraftStore
{
    ScheduledExceptionDraft? Find(string code);
    void Save(ScheduledExceptionDraft item);
    bool Remove(string code);
    List<ScheduledExceptionDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledExceptionDraftStore(Dictionary<string, ScheduledExceptionDraft> items, List<string> order) : IScheduledExceptionDraftStore
{
    public Dictionary<string, ScheduledExceptionDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledExceptionDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledExceptionDraft item)
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

    public List<ScheduledExceptionDraft> List()
    {
        var outValue = new List<ScheduledExceptionDraft>();
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
public sealed class CachedScheduledExceptionDraftStore(MemoryScheduledExceptionDraftStore inner, ScheduledExceptionDraftCache cache, int hits, int misses) : IScheduledExceptionDraftStore
{
    public MemoryScheduledExceptionDraftStore Inner { get; set; } = inner;
    public ScheduledExceptionDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledExceptionDraft? Find(string code)
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

    public void Save(ScheduledExceptionDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledExceptionDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled exception drafts. Empty fields match all.</summary>
public sealed record ScheduledExceptionDraftQuery(string Segment, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledExceptionDraft item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledExceptionDraft> Apply(List<ScheduledExceptionDraft> items)
    {
        var matched = Collections.Filter(items, (ScheduledExceptionDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled exception draft store.</summary>
public static class ScheduledExceptionDraftStore
{
    public static IScheduledExceptionDraftStore NewMemoryScheduledExceptionDraftStore()
    {
        return new MemoryScheduledExceptionDraftStore(
            new Dictionary<string, ScheduledExceptionDraft>(),
            new List<string>());
    }

    public static IScheduledExceptionDraftStore NewCachedScheduledExceptionDraftStore(MemoryScheduledExceptionDraftStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledExceptionDraftStore(
            inner,
            ScheduledExceptionDraftCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledExceptionDraft> FindScheduledExceptionDrafts(IScheduledExceptionDraftStore store, ScheduledExceptionDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
