namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle requests by code.</summary>
public interface IMeritCycleRequestStore
{
    MeritCycleRequest? Find(string code);
    void Save(MeritCycleRequest item);
    bool Remove(string code);
    List<MeritCycleRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleRequestStore(Dictionary<string, MeritCycleRequest> items, List<string> order) : IMeritCycleRequestStore
{
    public Dictionary<string, MeritCycleRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleRequest item)
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

    public List<MeritCycleRequest> List()
    {
        var outValue = new List<MeritCycleRequest>();
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
public sealed class CachedMeritCycleRequestStore(MemoryMeritCycleRequestStore inner, MeritCycleRequestCache cache, int hits, int misses) : IMeritCycleRequestStore
{
    public MemoryMeritCycleRequestStore Inner { get; set; } = inner;
    public MeritCycleRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleRequest? Find(string code)
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

    public void Save(MeritCycleRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle requests. Empty fields match all.</summary>
public sealed record MeritCycleRequestQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleRequest> Apply(List<MeritCycleRequest> items)
    {
        var matched = Collections.Filter(items, (MeritCycleRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle request store.</summary>
public static class MeritCycleRequestStore
{
    public static IMeritCycleRequestStore NewMemoryMeritCycleRequestStore()
    {
        return new MemoryMeritCycleRequestStore(new Dictionary<string, MeritCycleRequest>(), new List<string>());
    }

    public static IMeritCycleRequestStore NewCachedMeritCycleRequestStore(MemoryMeritCycleRequestStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleRequestStore(inner, MeritCycleRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MeritCycleRequest> FindMeritCycleRequests(IMeritCycleRequestStore store, MeritCycleRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
