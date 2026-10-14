namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending rate windows by code.</summary>
public interface IPendingRateWindowStore
{
    PendingRateWindow? Find(string code);
    void Save(PendingRateWindow item);
    bool Remove(string code);
    List<PendingRateWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingRateWindowStore(Dictionary<string, PendingRateWindow> items, List<string> order) : IPendingRateWindowStore
{
    public Dictionary<string, PendingRateWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingRateWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingRateWindow item)
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

    public List<PendingRateWindow> List()
    {
        var outValue = new List<PendingRateWindow>();
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
public sealed class CachedPendingRateWindowStore(MemoryPendingRateWindowStore inner, PendingRateWindowCache cache, int hits, int misses) : IPendingRateWindowStore
{
    public MemoryPendingRateWindowStore Inner { get; set; } = inner;
    public PendingRateWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingRateWindow? Find(string code)
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

    public void Save(PendingRateWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingRateWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending rate windows. Empty fields match all.</summary>
public sealed record PendingRateWindowQuery(string Channel, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingRateWindow item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingRateWindow> Apply(List<PendingRateWindow> items)
    {
        var matched = Collections.Filter(items, (PendingRateWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending rate window store.</summary>
public static class PendingRateWindowStore
{
    public static IPendingRateWindowStore NewMemoryPendingRateWindowStore()
    {
        return new MemoryPendingRateWindowStore(new Dictionary<string, PendingRateWindow>(), new List<string>());
    }

    public static IPendingRateWindowStore NewCachedPendingRateWindowStore(MemoryPendingRateWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPendingRateWindowStore(inner, PendingRateWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PendingRateWindow> FindPendingRateWindows(IPendingRateWindowStore store, PendingRateWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
