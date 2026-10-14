namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall digests by code.</summary>
public interface ITownHallDigestStore
{
    TownHallDigest? Find(string code);
    void Save(TownHallDigest item);
    bool Remove(string code);
    List<TownHallDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallDigestStore(Dictionary<string, TownHallDigest> items, List<string> order) : ITownHallDigestStore
{
    public Dictionary<string, TownHallDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallDigest item)
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

    public List<TownHallDigest> List()
    {
        var outValue = new List<TownHallDigest>();
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
public sealed class CachedTownHallDigestStore(MemoryTownHallDigestStore inner, TownHallDigestCache cache, int hits, int misses) : ITownHallDigestStore
{
    public MemoryTownHallDigestStore Inner { get; set; } = inner;
    public TownHallDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallDigest? Find(string code)
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

    public void Save(TownHallDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall digests. Empty fields match all.</summary>
public sealed record TownHallDigestQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallDigest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TownHallDigest> Apply(List<TownHallDigest> items)
    {
        var matched = Collections.Filter(items, (TownHallDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall digest store.</summary>
public static class TownHallDigestStore
{
    public static ITownHallDigestStore NewMemoryTownHallDigestStore()
    {
        return new MemoryTownHallDigestStore(new Dictionary<string, TownHallDigest>(), new List<string>());
    }

    public static ITownHallDigestStore NewCachedTownHallDigestStore(MemoryTownHallDigestStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallDigestStore(inner, TownHallDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TownHallDigest> FindTownHallDigests(ITownHallDigestStore store, TownHallDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
