namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback feedback entries by code.</summary>
public interface IFallbackFeedbackEntryStore
{
    FallbackFeedbackEntry? Find(string code);
    void Save(FallbackFeedbackEntry item);
    bool Remove(string code);
    List<FallbackFeedbackEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackFeedbackEntryStore(Dictionary<string, FallbackFeedbackEntry> items, List<string> order) : IFallbackFeedbackEntryStore
{
    public Dictionary<string, FallbackFeedbackEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackFeedbackEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackFeedbackEntry item)
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

    public List<FallbackFeedbackEntry> List()
    {
        var outValue = new List<FallbackFeedbackEntry>();
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
public sealed class CachedFallbackFeedbackEntryStore(MemoryFallbackFeedbackEntryStore inner, FallbackFeedbackEntryCache cache, int hits, int misses) : IFallbackFeedbackEntryStore
{
    public MemoryFallbackFeedbackEntryStore Inner { get; set; } = inner;
    public FallbackFeedbackEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackFeedbackEntry? Find(string code)
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

    public void Save(FallbackFeedbackEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackFeedbackEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback feedback entries. Empty fields match all.</summary>
public sealed record FallbackFeedbackEntryQuery(string Team, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackFeedbackEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackFeedbackEntry> Apply(List<FallbackFeedbackEntry> items)
    {
        var matched = Collections.Filter(items, (FallbackFeedbackEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback feedback entry store.</summary>
public static class FallbackFeedbackEntryStore
{
    public static IFallbackFeedbackEntryStore NewMemoryFallbackFeedbackEntryStore()
    {
        return new MemoryFallbackFeedbackEntryStore(
            new Dictionary<string, FallbackFeedbackEntry>(),
            new List<string>());
    }

    public static IFallbackFeedbackEntryStore NewCachedFallbackFeedbackEntryStore(MemoryFallbackFeedbackEntryStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackFeedbackEntryStore(inner, FallbackFeedbackEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FallbackFeedbackEntry> FindFallbackFeedbackEntries(IFallbackFeedbackEntryStore store, FallbackFeedbackEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
