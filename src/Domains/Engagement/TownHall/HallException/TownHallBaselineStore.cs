namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall baselines by code.</summary>
public interface ITownHallBaselineStore
{
    TownHallBaseline? Find(string code);
    void Save(TownHallBaseline item);
    bool Remove(string code);
    List<TownHallBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallBaselineStore(Dictionary<string, TownHallBaseline> items, List<string> order) : ITownHallBaselineStore
{
    public Dictionary<string, TownHallBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallBaseline item)
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

    public List<TownHallBaseline> List()
    {
        var outValue = new List<TownHallBaseline>();
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
public sealed class CachedTownHallBaselineStore(MemoryTownHallBaselineStore inner, TownHallBaselineCache cache, int hits, int misses) : ITownHallBaselineStore
{
    public MemoryTownHallBaselineStore Inner { get; set; } = inner;
    public TownHallBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallBaseline? Find(string code)
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

    public void Save(TownHallBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall baselines. Empty fields match all.</summary>
public sealed record TownHallBaselineQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallBaseline item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<TownHallBaseline> Apply(List<TownHallBaseline> items)
    {
        var matched = Collections.Filter(items, (TownHallBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall baseline store.</summary>
public static class TownHallBaselineStore
{
    public static ITownHallBaselineStore NewMemoryTownHallBaselineStore()
    {
        return new MemoryTownHallBaselineStore(new Dictionary<string, TownHallBaseline>(), new List<string>());
    }

    public static ITownHallBaselineStore NewCachedTownHallBaselineStore(MemoryTownHallBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallBaselineStore(inner, TownHallBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallBaseline> FindTownHallBaselines(ITownHallBaselineStore store, TownHallBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
