namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending town hall baselines by code.</summary>
public interface IPendingTownHallBaselineStore
{
    PendingTownHallBaseline? Find(string code);
    void Save(PendingTownHallBaseline item);
    bool Remove(string code);
    List<PendingTownHallBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTownHallBaselineStore(Dictionary<string, PendingTownHallBaseline> items, List<string> order) : IPendingTownHallBaselineStore
{
    public Dictionary<string, PendingTownHallBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTownHallBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTownHallBaseline item)
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

    public List<PendingTownHallBaseline> List()
    {
        var outValue = new List<PendingTownHallBaseline>();
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
public sealed class CachedPendingTownHallBaselineStore(MemoryPendingTownHallBaselineStore inner, PendingTownHallBaselineCache cache, int hits, int misses) : IPendingTownHallBaselineStore
{
    public MemoryPendingTownHallBaselineStore Inner { get; set; } = inner;
    public PendingTownHallBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTownHallBaseline? Find(string code)
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

    public void Save(PendingTownHallBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTownHallBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending town hall baselines. Empty fields match all.</summary>
public sealed record PendingTownHallBaselineQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTownHallBaseline item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTownHallBaseline> Apply(List<PendingTownHallBaseline> items)
    {
        var matched = Collections.Filter(items, (PendingTownHallBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending town hall baseline store.</summary>
public static class PendingTownHallBaselineStore
{
    public static IPendingTownHallBaselineStore NewMemoryPendingTownHallBaselineStore()
    {
        return new MemoryPendingTownHallBaselineStore(
            new Dictionary<string, PendingTownHallBaseline>(),
            new List<string>());
    }

    public static IPendingTownHallBaselineStore NewCachedPendingTownHallBaselineStore(MemoryPendingTownHallBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTownHallBaselineStore(
            inner,
            PendingTownHallBaselineCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingTownHallBaseline> FindPendingTownHallBaselines(IPendingTownHallBaselineStore store, PendingTownHallBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
