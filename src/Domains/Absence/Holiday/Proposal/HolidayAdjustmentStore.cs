namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday adjustments by code.</summary>
public interface IHolidayAdjustmentStore
{
    HolidayAdjustment? Find(string code);
    void Save(HolidayAdjustment item);
    bool Remove(string code);
    List<HolidayAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayAdjustmentStore(Dictionary<string, HolidayAdjustment> items, List<string> order) : IHolidayAdjustmentStore
{
    public Dictionary<string, HolidayAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayAdjustment item)
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

    public List<HolidayAdjustment> List()
    {
        var outValue = new List<HolidayAdjustment>();
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
public sealed class CachedHolidayAdjustmentStore(MemoryHolidayAdjustmentStore inner, HolidayAdjustmentCache cache, int hits, int misses) : IHolidayAdjustmentStore
{
    public MemoryHolidayAdjustmentStore Inner { get; set; } = inner;
    public HolidayAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayAdjustment? Find(string code)
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

    public void Save(HolidayAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday adjustments. Empty fields match all.</summary>
public sealed record HolidayAdjustmentQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<HolidayAdjustment> Apply(List<HolidayAdjustment> items)
    {
        var matched = Collections.Filter(items, (HolidayAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday adjustment store.</summary>
public static class HolidayAdjustmentStore
{
    public static IHolidayAdjustmentStore NewMemoryHolidayAdjustmentStore()
    {
        return new MemoryHolidayAdjustmentStore(new Dictionary<string, HolidayAdjustment>(), new List<string>());
    }

    public static IHolidayAdjustmentStore NewCachedHolidayAdjustmentStore(MemoryHolidayAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayAdjustmentStore(inner, HolidayAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayAdjustment> FindHolidayAdjustments(IHolidayAdjustmentStore store, HolidayAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
