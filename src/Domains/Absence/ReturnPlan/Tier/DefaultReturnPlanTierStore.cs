namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default return plan tiers by code.</summary>
public interface IDefaultReturnPlanTierStore
{
    DefaultReturnPlanTier? Find(string code);
    void Save(DefaultReturnPlanTier item);
    bool Remove(string code);
    List<DefaultReturnPlanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReturnPlanTierStore(Dictionary<string, DefaultReturnPlanTier> items, List<string> order) : IDefaultReturnPlanTierStore
{
    public Dictionary<string, DefaultReturnPlanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReturnPlanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReturnPlanTier item)
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

    public List<DefaultReturnPlanTier> List()
    {
        var outValue = new List<DefaultReturnPlanTier>();
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
public sealed class CachedDefaultReturnPlanTierStore(MemoryDefaultReturnPlanTierStore inner, DefaultReturnPlanTierCache cache, int hits, int misses) : IDefaultReturnPlanTierStore
{
    public MemoryDefaultReturnPlanTierStore Inner { get; set; } = inner;
    public DefaultReturnPlanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReturnPlanTier? Find(string code)
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

    public void Save(DefaultReturnPlanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReturnPlanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default return plan tiers. Empty fields match all.</summary>
public sealed record DefaultReturnPlanTierQuery(string Channel, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReturnPlanTier item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReturnPlanTier> Apply(List<DefaultReturnPlanTier> items)
    {
        var matched = Collections.Filter(items, (DefaultReturnPlanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default return plan tier store.</summary>
public static class DefaultReturnPlanTierStore
{
    public static IDefaultReturnPlanTierStore NewMemoryDefaultReturnPlanTierStore()
    {
        return new MemoryDefaultReturnPlanTierStore(
            new Dictionary<string, DefaultReturnPlanTier>(),
            new List<string>());
    }

    public static IDefaultReturnPlanTierStore NewCachedDefaultReturnPlanTierStore(MemoryDefaultReturnPlanTierStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReturnPlanTierStore(inner, DefaultReturnPlanTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultReturnPlanTier> FindDefaultReturnPlanTiers(IDefaultReturnPlanTierStore store, DefaultReturnPlanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
