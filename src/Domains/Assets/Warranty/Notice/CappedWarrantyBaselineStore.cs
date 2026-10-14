namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped warranty baselines by code.</summary>
public interface ICappedWarrantyBaselineStore
{
    CappedWarrantyBaseline? Find(string code);
    void Save(CappedWarrantyBaseline item);
    bool Remove(string code);
    List<CappedWarrantyBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedWarrantyBaselineStore(Dictionary<string, CappedWarrantyBaseline> items, List<string> order) : ICappedWarrantyBaselineStore
{
    public Dictionary<string, CappedWarrantyBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedWarrantyBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedWarrantyBaseline item)
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

    public List<CappedWarrantyBaseline> List()
    {
        var outValue = new List<CappedWarrantyBaseline>();
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
public sealed class CachedCappedWarrantyBaselineStore(MemoryCappedWarrantyBaselineStore inner, CappedWarrantyBaselineCache cache, int hits, int misses) : ICappedWarrantyBaselineStore
{
    public MemoryCappedWarrantyBaselineStore Inner { get; set; } = inner;
    public CappedWarrantyBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedWarrantyBaseline? Find(string code)
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

    public void Save(CappedWarrantyBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedWarrantyBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped warranty baselines. Empty fields match all.</summary>
public sealed record CappedWarrantyBaselineQuery(string Category, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedWarrantyBaseline item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedWarrantyBaseline> Apply(List<CappedWarrantyBaseline> items)
    {
        var matched = Collections.Filter(items, (CappedWarrantyBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped warranty baseline store.</summary>
public static class CappedWarrantyBaselineStore
{
    public static ICappedWarrantyBaselineStore NewMemoryCappedWarrantyBaselineStore()
    {
        return new MemoryCappedWarrantyBaselineStore(
            new Dictionary<string, CappedWarrantyBaseline>(),
            new List<string>());
    }

    public static ICappedWarrantyBaselineStore NewCachedCappedWarrantyBaselineStore(MemoryCappedWarrantyBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedCappedWarrantyBaselineStore(
            inner,
            CappedWarrantyBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedWarrantyBaseline> FindCappedWarrantyBaselines(ICappedWarrantyBaselineStore store, CappedWarrantyBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
