namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall quotas by code.</summary>
public interface ITownHallQuotaStore
{
    TownHallQuota? Find(string code);
    void Save(TownHallQuota item);
    bool Remove(string code);
    List<TownHallQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallQuotaStore(Dictionary<string, TownHallQuota> items, List<string> order) : ITownHallQuotaStore
{
    public Dictionary<string, TownHallQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallQuota item)
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

    public List<TownHallQuota> List()
    {
        var outValue = new List<TownHallQuota>();
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
public sealed class CachedTownHallQuotaStore(MemoryTownHallQuotaStore inner, TownHallQuotaCache cache, int hits, int misses) : ITownHallQuotaStore
{
    public MemoryTownHallQuotaStore Inner { get; set; } = inner;
    public TownHallQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallQuota? Find(string code)
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

    public void Save(TownHallQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall quotas. Empty fields match all.</summary>
public sealed record TownHallQuotaQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallQuota item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<TownHallQuota> Apply(List<TownHallQuota> items)
    {
        var matched = Collections.Filter(items, (TownHallQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall quota store.</summary>
public static class TownHallQuotaStore
{
    public static ITownHallQuotaStore NewMemoryTownHallQuotaStore()
    {
        return new MemoryTownHallQuotaStore(new Dictionary<string, TownHallQuota>(), new List<string>());
    }

    public static ITownHallQuotaStore NewCachedTownHallQuotaStore(MemoryTownHallQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallQuotaStore(inner, TownHallQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallQuota> FindTownHallQuotas(ITownHallQuotaStore store, TownHallQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
