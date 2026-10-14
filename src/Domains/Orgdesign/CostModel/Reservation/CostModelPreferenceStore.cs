namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model preferences by code.</summary>
public interface ICostModelPreferenceStore
{
    CostModelPreference? Find(string code);
    void Save(CostModelPreference item);
    bool Remove(string code);
    List<CostModelPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelPreferenceStore(Dictionary<string, CostModelPreference> items, List<string> order) : ICostModelPreferenceStore
{
    public Dictionary<string, CostModelPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelPreference item)
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

    public List<CostModelPreference> List()
    {
        var outValue = new List<CostModelPreference>();
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
public sealed class CachedCostModelPreferenceStore(MemoryCostModelPreferenceStore inner, CostModelPreferenceCache cache, int hits, int misses) : ICostModelPreferenceStore
{
    public MemoryCostModelPreferenceStore Inner { get; set; } = inner;
    public CostModelPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelPreference? Find(string code)
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

    public void Save(CostModelPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model preferences. Empty fields match all.</summary>
public sealed record CostModelPreferenceQuery(string Region, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelPreference item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelPreference> Apply(List<CostModelPreference> items)
    {
        var matched = Collections.Filter(items, (CostModelPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model preference store.</summary>
public static class CostModelPreferenceStore
{
    public static ICostModelPreferenceStore NewMemoryCostModelPreferenceStore()
    {
        return new MemoryCostModelPreferenceStore(new Dictionary<string, CostModelPreference>(), new List<string>());
    }

    public static ICostModelPreferenceStore NewCachedCostModelPreferenceStore(MemoryCostModelPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelPreferenceStore(inner, CostModelPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CostModelPreference> FindCostModelPreferences(ICostModelPreferenceStore store, CostModelPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
