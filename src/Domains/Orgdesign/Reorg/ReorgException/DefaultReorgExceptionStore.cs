namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default reorg exceptions by code.</summary>
public interface IDefaultReorgExceptionStore
{
    DefaultReorgException? Find(string code);
    void Save(DefaultReorgException item);
    bool Remove(string code);
    List<DefaultReorgException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReorgExceptionStore(Dictionary<string, DefaultReorgException> items, List<string> order) : IDefaultReorgExceptionStore
{
    public Dictionary<string, DefaultReorgException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReorgException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReorgException item)
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

    public List<DefaultReorgException> List()
    {
        var outValue = new List<DefaultReorgException>();
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
public sealed class CachedDefaultReorgExceptionStore(MemoryDefaultReorgExceptionStore inner, DefaultReorgExceptionCache cache, int hits, int misses) : IDefaultReorgExceptionStore
{
    public MemoryDefaultReorgExceptionStore Inner { get; set; } = inner;
    public DefaultReorgExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReorgException? Find(string code)
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

    public void Save(DefaultReorgException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReorgException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default reorg exceptions. Empty fields match all.</summary>
public sealed record DefaultReorgExceptionQuery(string Country, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReorgException item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReorgException> Apply(List<DefaultReorgException> items)
    {
        var matched = Collections.Filter(items, (DefaultReorgException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default reorg exception store.</summary>
public static class DefaultReorgExceptionStore
{
    public static IDefaultReorgExceptionStore NewMemoryDefaultReorgExceptionStore()
    {
        return new MemoryDefaultReorgExceptionStore(
            new Dictionary<string, DefaultReorgException>(),
            new List<string>());
    }

    public static IDefaultReorgExceptionStore NewCachedDefaultReorgExceptionStore(MemoryDefaultReorgExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReorgExceptionStore(inner, DefaultReorgExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultReorgException> FindDefaultReorgExceptions(IDefaultReorgExceptionStore store, DefaultReorgExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
