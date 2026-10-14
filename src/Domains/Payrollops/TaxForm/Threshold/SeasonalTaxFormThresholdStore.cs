namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal tax form thresholds by code.</summary>
public interface ISeasonalTaxFormThresholdStore
{
    SeasonalTaxFormThreshold? Find(string code);
    void Save(SeasonalTaxFormThreshold item);
    bool Remove(string code);
    List<SeasonalTaxFormThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalTaxFormThresholdStore(Dictionary<string, SeasonalTaxFormThreshold> items, List<string> order) : ISeasonalTaxFormThresholdStore
{
    public Dictionary<string, SeasonalTaxFormThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalTaxFormThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalTaxFormThreshold item)
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

    public List<SeasonalTaxFormThreshold> List()
    {
        var outValue = new List<SeasonalTaxFormThreshold>();
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
public sealed class CachedSeasonalTaxFormThresholdStore(MemorySeasonalTaxFormThresholdStore inner, SeasonalTaxFormThresholdCache cache, int hits, int misses) : ISeasonalTaxFormThresholdStore
{
    public MemorySeasonalTaxFormThresholdStore Inner { get; set; } = inner;
    public SeasonalTaxFormThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalTaxFormThreshold? Find(string code)
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

    public void Save(SeasonalTaxFormThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalTaxFormThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal tax form thresholds. Empty fields match all.</summary>
public sealed record SeasonalTaxFormThresholdQuery(string Department, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalTaxFormThreshold item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SeasonalTaxFormThreshold> Apply(List<SeasonalTaxFormThreshold> items)
    {
        var matched = Collections.Filter(items, (SeasonalTaxFormThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal tax form threshold store.</summary>
public static class SeasonalTaxFormThresholdStore
{
    public static ISeasonalTaxFormThresholdStore NewMemorySeasonalTaxFormThresholdStore()
    {
        return new MemorySeasonalTaxFormThresholdStore(
            new Dictionary<string, SeasonalTaxFormThreshold>(),
            new List<string>());
    }

    public static ISeasonalTaxFormThresholdStore NewCachedSeasonalTaxFormThresholdStore(MemorySeasonalTaxFormThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalTaxFormThresholdStore(
            inner,
            SeasonalTaxFormThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalTaxFormThreshold> FindSeasonalTaxFormThresholds(ISeasonalTaxFormThresholdStore store, SeasonalTaxFormThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
