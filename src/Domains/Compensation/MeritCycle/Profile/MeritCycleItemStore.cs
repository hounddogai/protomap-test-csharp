namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle items by code.</summary>
public interface IMeritCycleItemStore
{
    MeritCycleItem? Find(string code);
    void Save(MeritCycleItem item);
    bool Remove(string code);
    List<MeritCycleItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleItemStore(Dictionary<string, MeritCycleItem> items, List<string> order) : IMeritCycleItemStore
{
    public Dictionary<string, MeritCycleItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleItem item)
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

    public List<MeritCycleItem> List()
    {
        var outValue = new List<MeritCycleItem>();
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
public sealed class CachedMeritCycleItemStore(MemoryMeritCycleItemStore inner, MeritCycleItemCache cache, int hits, int misses) : IMeritCycleItemStore
{
    public MemoryMeritCycleItemStore Inner { get; set; } = inner;
    public MeritCycleItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleItem? Find(string code)
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

    public void Save(MeritCycleItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle items. Empty fields match all.</summary>
public sealed record MeritCycleItemQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleItem item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleItem> Apply(List<MeritCycleItem> items)
    {
        var matched = Collections.Filter(items, (MeritCycleItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle item store.</summary>
public static class MeritCycleItemStore
{
    public static IMeritCycleItemStore NewMemoryMeritCycleItemStore()
    {
        return new MemoryMeritCycleItemStore(new Dictionary<string, MeritCycleItem>(), new List<string>());
    }

    public static IMeritCycleItemStore NewCachedMeritCycleItemStore(MemoryMeritCycleItemStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleItemStore(inner, MeritCycleItemCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleItem> FindMeritCycleItems(IMeritCycleItemStore store, MeritCycleItemQuery query)
    {
        return query.Apply(store.List());
    }
}
