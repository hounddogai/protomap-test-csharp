namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay run requests by code.</summary>
public interface ILegacyPayRunRequestStore
{
    LegacyPayRunRequest? Find(string code);
    void Save(LegacyPayRunRequest item);
    bool Remove(string code);
    List<LegacyPayRunRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayRunRequestStore(Dictionary<string, LegacyPayRunRequest> items, List<string> order) : ILegacyPayRunRequestStore
{
    public Dictionary<string, LegacyPayRunRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayRunRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayRunRequest item)
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

    public List<LegacyPayRunRequest> List()
    {
        var outValue = new List<LegacyPayRunRequest>();
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
public sealed class CachedLegacyPayRunRequestStore(MemoryLegacyPayRunRequestStore inner, LegacyPayRunRequestCache cache, int hits, int misses) : ILegacyPayRunRequestStore
{
    public MemoryLegacyPayRunRequestStore Inner { get; set; } = inner;
    public LegacyPayRunRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayRunRequest? Find(string code)
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

    public void Save(LegacyPayRunRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayRunRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay run requests. Empty fields match all.</summary>
public sealed record LegacyPayRunRequestQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayRunRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPayRunRequest> Apply(List<LegacyPayRunRequest> items)
    {
        var matched = Collections.Filter(items, (LegacyPayRunRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay run request store.</summary>
public static class LegacyPayRunRequestStore
{
    public static ILegacyPayRunRequestStore NewMemoryLegacyPayRunRequestStore()
    {
        return new MemoryLegacyPayRunRequestStore(new Dictionary<string, LegacyPayRunRequest>(), new List<string>());
    }

    public static ILegacyPayRunRequestStore NewCachedLegacyPayRunRequestStore(MemoryLegacyPayRunRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayRunRequestStore(inner, LegacyPayRunRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyPayRunRequest> FindLegacyPayRunRequests(ILegacyPayRunRequestStore store, LegacyPayRunRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
