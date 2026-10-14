namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict tax form groups by code.</summary>
public interface IStrictTaxFormGroupStore
{
    StrictTaxFormGroup? Find(string code);
    void Save(StrictTaxFormGroup item);
    bool Remove(string code);
    List<StrictTaxFormGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictTaxFormGroupStore(Dictionary<string, StrictTaxFormGroup> items, List<string> order) : IStrictTaxFormGroupStore
{
    public Dictionary<string, StrictTaxFormGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictTaxFormGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictTaxFormGroup item)
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

    public List<StrictTaxFormGroup> List()
    {
        var outValue = new List<StrictTaxFormGroup>();
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
public sealed class CachedStrictTaxFormGroupStore(MemoryStrictTaxFormGroupStore inner, StrictTaxFormGroupCache cache, int hits, int misses) : IStrictTaxFormGroupStore
{
    public MemoryStrictTaxFormGroupStore Inner { get; set; } = inner;
    public StrictTaxFormGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictTaxFormGroup? Find(string code)
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

    public void Save(StrictTaxFormGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictTaxFormGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict tax form groups. Empty fields match all.</summary>
public sealed record StrictTaxFormGroupQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(StrictTaxFormGroup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictTaxFormGroup> Apply(List<StrictTaxFormGroup> items)
    {
        var matched = Collections.Filter(items, (StrictTaxFormGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict tax form group store.</summary>
public static class StrictTaxFormGroupStore
{
    public static IStrictTaxFormGroupStore NewMemoryStrictTaxFormGroupStore()
    {
        return new MemoryStrictTaxFormGroupStore(new Dictionary<string, StrictTaxFormGroup>(), new List<string>());
    }

    public static IStrictTaxFormGroupStore NewCachedStrictTaxFormGroupStore(MemoryStrictTaxFormGroupStore inner, int ttl, IClock clock)
    {
        return new CachedStrictTaxFormGroupStore(inner, StrictTaxFormGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictTaxFormGroup> FindStrictTaxFormGroups(IStrictTaxFormGroupStore store, StrictTaxFormGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
