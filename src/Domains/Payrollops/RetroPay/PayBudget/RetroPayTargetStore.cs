namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay targets by code.</summary>
public interface IRetroPayTargetStore
{
    RetroPayTarget? Find(string code);
    void Save(RetroPayTarget item);
    bool Remove(string code);
    List<RetroPayTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayTargetStore(Dictionary<string, RetroPayTarget> items, List<string> order) : IRetroPayTargetStore
{
    public Dictionary<string, RetroPayTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayTarget item)
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

    public List<RetroPayTarget> List()
    {
        var outValue = new List<RetroPayTarget>();
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
public sealed class CachedRetroPayTargetStore(MemoryRetroPayTargetStore inner, RetroPayTargetCache cache, int hits, int misses) : IRetroPayTargetStore
{
    public MemoryRetroPayTargetStore Inner { get; set; } = inner;
    public RetroPayTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayTarget? Find(string code)
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

    public void Save(RetroPayTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay targets. Empty fields match all.</summary>
public sealed record RetroPayTargetQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayTarget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayTarget> Apply(List<RetroPayTarget> items)
    {
        var matched = Collections.Filter(items, (RetroPayTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay target store.</summary>
public static class RetroPayTargetStore
{
    public static IRetroPayTargetStore NewMemoryRetroPayTargetStore()
    {
        return new MemoryRetroPayTargetStore(new Dictionary<string, RetroPayTarget>(), new List<string>());
    }

    public static IRetroPayTargetStore NewCachedRetroPayTargetStore(MemoryRetroPayTargetStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayTargetStore(inner, RetroPayTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayTarget> FindRetroPayTargets(IRetroPayTargetStore store, RetroPayTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
