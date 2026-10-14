namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh rules by code.</summary>
public interface IRefreshRuleStore
{
    RefreshRule? Find(string code);
    void Save(RefreshRule item);
    bool Remove(string code);
    List<RefreshRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshRuleStore(Dictionary<string, RefreshRule> items, List<string> order) : IRefreshRuleStore
{
    public Dictionary<string, RefreshRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshRule item)
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

    public List<RefreshRule> List()
    {
        var outValue = new List<RefreshRule>();
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
public sealed class CachedRefreshRuleStore(MemoryRefreshRuleStore inner, RefreshRuleCache cache, int hits, int misses) : IRefreshRuleStore
{
    public MemoryRefreshRuleStore Inner { get; set; } = inner;
    public RefreshRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshRule? Find(string code)
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

    public void Save(RefreshRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh rules. Empty fields match all.</summary>
public sealed record RefreshRuleQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshRule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshRule> Apply(List<RefreshRule> items)
    {
        var matched = Collections.Filter(items, (RefreshRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh rule store.</summary>
public static class RefreshRuleStore
{
    public static IRefreshRuleStore NewMemoryRefreshRuleStore()
    {
        return new MemoryRefreshRuleStore(new Dictionary<string, RefreshRule>(), new List<string>());
    }

    public static IRefreshRuleStore NewCachedRefreshRuleStore(MemoryRefreshRuleStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshRuleStore(inner, RefreshRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshRule> FindRefreshRules(IRefreshRuleStore store, RefreshRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
