namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict tax form bundles by code.</summary>
public interface IStrictTaxFormBundleStore
{
    StrictTaxFormBundle? Find(string code);
    void Save(StrictTaxFormBundle item);
    bool Remove(string code);
    List<StrictTaxFormBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictTaxFormBundleStore(Dictionary<string, StrictTaxFormBundle> items, List<string> order) : IStrictTaxFormBundleStore
{
    public Dictionary<string, StrictTaxFormBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictTaxFormBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictTaxFormBundle item)
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

    public List<StrictTaxFormBundle> List()
    {
        var outValue = new List<StrictTaxFormBundle>();
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
public sealed class CachedStrictTaxFormBundleStore(MemoryStrictTaxFormBundleStore inner, StrictTaxFormBundleCache cache, int hits, int misses) : IStrictTaxFormBundleStore
{
    public MemoryStrictTaxFormBundleStore Inner { get; set; } = inner;
    public StrictTaxFormBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictTaxFormBundle? Find(string code)
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

    public void Save(StrictTaxFormBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictTaxFormBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict tax form bundles. Empty fields match all.</summary>
public sealed record StrictTaxFormBundleQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictTaxFormBundle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictTaxFormBundle> Apply(List<StrictTaxFormBundle> items)
    {
        var matched = Collections.Filter(items, (StrictTaxFormBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict tax form bundle store.</summary>
public static class StrictTaxFormBundleStore
{
    public static IStrictTaxFormBundleStore NewMemoryStrictTaxFormBundleStore()
    {
        return new MemoryStrictTaxFormBundleStore(new Dictionary<string, StrictTaxFormBundle>(), new List<string>());
    }

    public static IStrictTaxFormBundleStore NewCachedStrictTaxFormBundleStore(MemoryStrictTaxFormBundleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictTaxFormBundleStore(inner, StrictTaxFormBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictTaxFormBundle> FindStrictTaxFormBundles(IStrictTaxFormBundleStore store, StrictTaxFormBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
