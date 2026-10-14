namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh targets by code.</summary>
public interface IRefreshTargetStore
{
    RefreshTarget? Find(string code);
    void Save(RefreshTarget item);
    bool Remove(string code);
    List<RefreshTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshTargetStore(Dictionary<string, RefreshTarget> items, List<string> order) : IRefreshTargetStore
{
    public Dictionary<string, RefreshTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshTarget item)
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

    public List<RefreshTarget> List()
    {
        var outValue = new List<RefreshTarget>();
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
public sealed class CachedRefreshTargetStore(MemoryRefreshTargetStore inner, RefreshTargetCache cache, int hits, int misses) : IRefreshTargetStore
{
    public MemoryRefreshTargetStore Inner { get; set; } = inner;
    public RefreshTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshTarget? Find(string code)
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

    public void Save(RefreshTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh targets. Empty fields match all.</summary>
public sealed record RefreshTargetQuery(string Unit, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshTarget> Apply(List<RefreshTarget> items)
    {
        var matched = Collections.Filter(items, (RefreshTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh target store.</summary>
public static class RefreshTargetStore
{
    public static IRefreshTargetStore NewMemoryRefreshTargetStore()
    {
        return new MemoryRefreshTargetStore(new Dictionary<string, RefreshTarget>(), new List<string>());
    }

    public static IRefreshTargetStore NewCachedRefreshTargetStore(MemoryRefreshTargetStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshTargetStore(inner, RefreshTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshTarget> FindRefreshTargets(IRefreshTargetStore store, RefreshTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
