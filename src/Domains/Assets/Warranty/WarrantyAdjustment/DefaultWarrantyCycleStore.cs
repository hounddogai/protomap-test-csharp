namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default warranty cycles by code.</summary>
public interface IDefaultWarrantyCycleStore
{
    DefaultWarrantyCycle? Find(string code);
    void Save(DefaultWarrantyCycle item);
    bool Remove(string code);
    List<DefaultWarrantyCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultWarrantyCycleStore(Dictionary<string, DefaultWarrantyCycle> items, List<string> order) : IDefaultWarrantyCycleStore
{
    public Dictionary<string, DefaultWarrantyCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultWarrantyCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultWarrantyCycle item)
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

    public List<DefaultWarrantyCycle> List()
    {
        var outValue = new List<DefaultWarrantyCycle>();
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
public sealed class CachedDefaultWarrantyCycleStore(MemoryDefaultWarrantyCycleStore inner, DefaultWarrantyCycleCache cache, int hits, int misses) : IDefaultWarrantyCycleStore
{
    public MemoryDefaultWarrantyCycleStore Inner { get; set; } = inner;
    public DefaultWarrantyCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultWarrantyCycle? Find(string code)
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

    public void Save(DefaultWarrantyCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultWarrantyCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default warranty cycles. Empty fields match all.</summary>
public sealed record DefaultWarrantyCycleQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultWarrantyCycle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultWarrantyCycle> Apply(List<DefaultWarrantyCycle> items)
    {
        var matched = Collections.Filter(items, (DefaultWarrantyCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default warranty cycle store.</summary>
public static class DefaultWarrantyCycleStore
{
    public static IDefaultWarrantyCycleStore NewMemoryDefaultWarrantyCycleStore()
    {
        return new MemoryDefaultWarrantyCycleStore(
            new Dictionary<string, DefaultWarrantyCycle>(),
            new List<string>());
    }

    public static IDefaultWarrantyCycleStore NewCachedDefaultWarrantyCycleStore(MemoryDefaultWarrantyCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultWarrantyCycleStore(inner, DefaultWarrantyCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultWarrantyCycle> FindDefaultWarrantyCycles(IDefaultWarrantyCycleStore store, DefaultWarrantyCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
