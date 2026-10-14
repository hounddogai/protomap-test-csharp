namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal holiday adjustments by code.</summary>
public interface ISeasonalHolidayAdjustmentStore
{
    SeasonalHolidayAdjustment? Find(string code);
    void Save(SeasonalHolidayAdjustment item);
    bool Remove(string code);
    List<SeasonalHolidayAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHolidayAdjustmentStore(Dictionary<string, SeasonalHolidayAdjustment> items, List<string> order) : ISeasonalHolidayAdjustmentStore
{
    public Dictionary<string, SeasonalHolidayAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHolidayAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHolidayAdjustment item)
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

    public List<SeasonalHolidayAdjustment> List()
    {
        var outValue = new List<SeasonalHolidayAdjustment>();
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
public sealed class CachedSeasonalHolidayAdjustmentStore(MemorySeasonalHolidayAdjustmentStore inner, SeasonalHolidayAdjustmentCache cache, int hits, int misses) : ISeasonalHolidayAdjustmentStore
{
    public MemorySeasonalHolidayAdjustmentStore Inner { get; set; } = inner;
    public SeasonalHolidayAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHolidayAdjustment? Find(string code)
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

    public void Save(SeasonalHolidayAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHolidayAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal holiday adjustments. Empty fields match all.</summary>
public sealed record SeasonalHolidayAdjustmentQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHolidayAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalHolidayAdjustment> Apply(List<SeasonalHolidayAdjustment> items)
    {
        var matched = Collections.Filter(items, (SeasonalHolidayAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal holiday adjustment store.</summary>
public static class SeasonalHolidayAdjustmentStore
{
    public static ISeasonalHolidayAdjustmentStore NewMemorySeasonalHolidayAdjustmentStore()
    {
        return new MemorySeasonalHolidayAdjustmentStore(
            new Dictionary<string, SeasonalHolidayAdjustment>(),
            new List<string>());
    }

    public static ISeasonalHolidayAdjustmentStore NewCachedSeasonalHolidayAdjustmentStore(MemorySeasonalHolidayAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHolidayAdjustmentStore(
            inner,
            SeasonalHolidayAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalHolidayAdjustment> FindSeasonalHolidayAdjustments(ISeasonalHolidayAdjustmentStore store, SeasonalHolidayAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
