namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan digests by code.</summary>
public interface IActionPlanDigestStore
{
    ActionPlanDigest? Find(string code);
    void Save(ActionPlanDigest item);
    bool Remove(string code);
    List<ActionPlanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanDigestStore(Dictionary<string, ActionPlanDigest> items, List<string> order) : IActionPlanDigestStore
{
    public Dictionary<string, ActionPlanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanDigest item)
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

    public List<ActionPlanDigest> List()
    {
        var outValue = new List<ActionPlanDigest>();
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
public sealed class CachedActionPlanDigestStore(MemoryActionPlanDigestStore inner, ActionPlanDigestCache cache, int hits, int misses) : IActionPlanDigestStore
{
    public MemoryActionPlanDigestStore Inner { get; set; } = inner;
    public ActionPlanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanDigest? Find(string code)
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

    public void Save(ActionPlanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan digests. Empty fields match all.</summary>
public sealed record ActionPlanDigestQuery(string Channel, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanDigest> Apply(List<ActionPlanDigest> items)
    {
        var matched = Collections.Filter(items, (ActionPlanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan digest store.</summary>
public static class ActionPlanDigestStore
{
    public static IActionPlanDigestStore NewMemoryActionPlanDigestStore()
    {
        return new MemoryActionPlanDigestStore(new Dictionary<string, ActionPlanDigest>(), new List<string>());
    }

    public static IActionPlanDigestStore NewCachedActionPlanDigestStore(MemoryActionPlanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanDigestStore(inner, ActionPlanDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ActionPlanDigest> FindActionPlanDigests(IActionPlanDigestStore store, ActionPlanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
