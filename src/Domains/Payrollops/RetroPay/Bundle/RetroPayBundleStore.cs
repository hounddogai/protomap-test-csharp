namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay bundles by code.</summary>
public interface IRetroPayBundleStore
{
    RetroPayBundle? Find(string code);
    void Save(RetroPayBundle item);
    bool Remove(string code);
    List<RetroPayBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayBundleStore(Dictionary<string, RetroPayBundle> items, List<string> order) : IRetroPayBundleStore
{
    public Dictionary<string, RetroPayBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayBundle item)
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

    public List<RetroPayBundle> List()
    {
        var outValue = new List<RetroPayBundle>();
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
public sealed class CachedRetroPayBundleStore(MemoryRetroPayBundleStore inner, RetroPayBundleCache cache, int hits, int misses) : IRetroPayBundleStore
{
    public MemoryRetroPayBundleStore Inner { get; set; } = inner;
    public RetroPayBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayBundle? Find(string code)
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

    public void Save(RetroPayBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay bundles. Empty fields match all.</summary>
public sealed record RetroPayBundleQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<RetroPayBundle> Apply(List<RetroPayBundle> items)
    {
        var matched = Collections.Filter(items, (RetroPayBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay bundle store.</summary>
public static class RetroPayBundleStore
{
    public static IRetroPayBundleStore NewMemoryRetroPayBundleStore()
    {
        return new MemoryRetroPayBundleStore(new Dictionary<string, RetroPayBundle>(), new List<string>());
    }

    public static IRetroPayBundleStore NewCachedRetroPayBundleStore(MemoryRetroPayBundleStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayBundleStore(inner, RetroPayBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayBundle> FindRetroPayBundles(IRetroPayBundleStore store, RetroPayBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
