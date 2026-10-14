namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan profiles by code.</summary>
public interface IActionPlanProfileStore
{
    ActionPlanProfile? Find(string code);
    void Save(ActionPlanProfile item);
    bool Remove(string code);
    List<ActionPlanProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanProfileStore(Dictionary<string, ActionPlanProfile> items, List<string> order) : IActionPlanProfileStore
{
    public Dictionary<string, ActionPlanProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanProfile item)
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

    public List<ActionPlanProfile> List()
    {
        var outValue = new List<ActionPlanProfile>();
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
public sealed class CachedActionPlanProfileStore(MemoryActionPlanProfileStore inner, ActionPlanProfileCache cache, int hits, int misses) : IActionPlanProfileStore
{
    public MemoryActionPlanProfileStore Inner { get; set; } = inner;
    public ActionPlanProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanProfile? Find(string code)
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

    public void Save(ActionPlanProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan profiles. Empty fields match all.</summary>
public sealed record ActionPlanProfileQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanProfile> Apply(List<ActionPlanProfile> items)
    {
        var matched = Collections.Filter(items, (ActionPlanProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan profile store.</summary>
public static class ActionPlanProfileStore
{
    public static IActionPlanProfileStore NewMemoryActionPlanProfileStore()
    {
        return new MemoryActionPlanProfileStore(new Dictionary<string, ActionPlanProfile>(), new List<string>());
    }

    public static IActionPlanProfileStore NewCachedActionPlanProfileStore(MemoryActionPlanProfileStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanProfileStore(inner, ActionPlanProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanProfile> FindActionPlanProfiles(IActionPlanProfileStore store, ActionPlanProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
