namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending town hall summaries by code.</summary>
public interface IPendingTownHallSummaryStore
{
    PendingTownHallSummary? Find(string code);
    void Save(PendingTownHallSummary item);
    bool Remove(string code);
    List<PendingTownHallSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTownHallSummaryStore(Dictionary<string, PendingTownHallSummary> items, List<string> order) : IPendingTownHallSummaryStore
{
    public Dictionary<string, PendingTownHallSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTownHallSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTownHallSummary item)
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

    public List<PendingTownHallSummary> List()
    {
        var outValue = new List<PendingTownHallSummary>();
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
public sealed class CachedPendingTownHallSummaryStore(MemoryPendingTownHallSummaryStore inner, PendingTownHallSummaryCache cache, int hits, int misses) : IPendingTownHallSummaryStore
{
    public MemoryPendingTownHallSummaryStore Inner { get; set; } = inner;
    public PendingTownHallSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTownHallSummary? Find(string code)
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

    public void Save(PendingTownHallSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTownHallSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending town hall summaries. Empty fields match all.</summary>
public sealed record PendingTownHallSummaryQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTownHallSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PendingTownHallSummary> Apply(List<PendingTownHallSummary> items)
    {
        var matched = Collections.Filter(items, (PendingTownHallSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending town hall summary store.</summary>
public static class PendingTownHallSummaryStore
{
    public static IPendingTownHallSummaryStore NewMemoryPendingTownHallSummaryStore()
    {
        return new MemoryPendingTownHallSummaryStore(
            new Dictionary<string, PendingTownHallSummary>(),
            new List<string>());
    }

    public static IPendingTownHallSummaryStore NewCachedPendingTownHallSummaryStore(MemoryPendingTownHallSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTownHallSummaryStore(
            inner,
            PendingTownHallSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingTownHallSummary> FindPendingTownHallSummaries(IPendingTownHallSummaryStore store, PendingTownHallSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
