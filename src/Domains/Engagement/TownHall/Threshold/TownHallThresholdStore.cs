namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall thresholds by code.</summary>
public interface ITownHallThresholdStore
{
    TownHallThreshold? Find(string code);
    void Save(TownHallThreshold item);
    bool Remove(string code);
    List<TownHallThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallThresholdStore(Dictionary<string, TownHallThreshold> items, List<string> order) : ITownHallThresholdStore
{
    public Dictionary<string, TownHallThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallThreshold item)
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

    public List<TownHallThreshold> List()
    {
        var outValue = new List<TownHallThreshold>();
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
public sealed class CachedTownHallThresholdStore(MemoryTownHallThresholdStore inner, TownHallThresholdCache cache, int hits, int misses) : ITownHallThresholdStore
{
    public MemoryTownHallThresholdStore Inner { get; set; } = inner;
    public TownHallThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallThreshold? Find(string code)
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

    public void Save(TownHallThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall thresholds. Empty fields match all.</summary>
public sealed record TownHallThresholdQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallThreshold> Apply(List<TownHallThreshold> items)
    {
        var matched = Collections.Filter(items, (TownHallThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall threshold store.</summary>
public static class TownHallThresholdStore
{
    public static ITownHallThresholdStore NewMemoryTownHallThresholdStore()
    {
        return new MemoryTownHallThresholdStore(new Dictionary<string, TownHallThreshold>(), new List<string>());
    }

    public static ITownHallThresholdStore NewCachedTownHallThresholdStore(MemoryTownHallThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallThresholdStore(inner, TownHallThresholdCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TownHallThreshold> FindTownHallThresholds(ITownHallThresholdStore store, TownHallThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
