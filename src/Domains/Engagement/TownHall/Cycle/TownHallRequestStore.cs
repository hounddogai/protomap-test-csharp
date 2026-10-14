namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall requests by code.</summary>
public interface ITownHallRequestStore
{
    TownHallRequest? Find(string code);
    void Save(TownHallRequest item);
    bool Remove(string code);
    List<TownHallRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallRequestStore(Dictionary<string, TownHallRequest> items, List<string> order) : ITownHallRequestStore
{
    public Dictionary<string, TownHallRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallRequest item)
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

    public List<TownHallRequest> List()
    {
        var outValue = new List<TownHallRequest>();
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
public sealed class CachedTownHallRequestStore(MemoryTownHallRequestStore inner, TownHallRequestCache cache, int hits, int misses) : ITownHallRequestStore
{
    public MemoryTownHallRequestStore Inner { get; set; } = inner;
    public TownHallRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallRequest? Find(string code)
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

    public void Save(TownHallRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall requests. Empty fields match all.</summary>
public sealed record TownHallRequestQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallRequest> Apply(List<TownHallRequest> items)
    {
        var matched = Collections.Filter(items, (TownHallRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall request store.</summary>
public static class TownHallRequestStore
{
    public static ITownHallRequestStore NewMemoryTownHallRequestStore()
    {
        return new MemoryTownHallRequestStore(new Dictionary<string, TownHallRequest>(), new List<string>());
    }

    public static ITownHallRequestStore NewCachedTownHallRequestStore(MemoryTownHallRequestStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallRequestStore(inner, TownHallRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TownHallRequest> FindTownHallRequests(ITownHallRequestStore store, TownHallRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
