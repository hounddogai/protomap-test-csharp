namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual windows by code.</summary>
public interface IAccrualWindowStore
{
    AccrualWindow? Find(string code);
    void Save(AccrualWindow item);
    bool Remove(string code);
    List<AccrualWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualWindowStore(Dictionary<string, AccrualWindow> items, List<string> order) : IAccrualWindowStore
{
    public Dictionary<string, AccrualWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualWindow item)
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

    public List<AccrualWindow> List()
    {
        var outValue = new List<AccrualWindow>();
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
public sealed class CachedAccrualWindowStore(MemoryAccrualWindowStore inner, AccrualWindowCache cache, int hits, int misses) : IAccrualWindowStore
{
    public MemoryAccrualWindowStore Inner { get; set; } = inner;
    public AccrualWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualWindow? Find(string code)
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

    public void Save(AccrualWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual windows. Empty fields match all.</summary>
public sealed record AccrualWindowQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualWindow item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualWindow> Apply(List<AccrualWindow> items)
    {
        var matched = Collections.Filter(items, (AccrualWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual window store.</summary>
public static class AccrualWindowStore
{
    public static IAccrualWindowStore NewMemoryAccrualWindowStore()
    {
        return new MemoryAccrualWindowStore(new Dictionary<string, AccrualWindow>(), new List<string>());
    }

    public static IAccrualWindowStore NewCachedAccrualWindowStore(MemoryAccrualWindowStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualWindowStore(inner, AccrualWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualWindow> FindAccrualWindows(IAccrualWindowStore store, AccrualWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
