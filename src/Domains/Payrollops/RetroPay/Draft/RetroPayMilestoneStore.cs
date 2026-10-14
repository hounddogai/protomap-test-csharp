namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay milestones by code.</summary>
public interface IRetroPayMilestoneStore
{
    RetroPayMilestone? Find(string code);
    void Save(RetroPayMilestone item);
    bool Remove(string code);
    List<RetroPayMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayMilestoneStore(Dictionary<string, RetroPayMilestone> items, List<string> order) : IRetroPayMilestoneStore
{
    public Dictionary<string, RetroPayMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayMilestone item)
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

    public List<RetroPayMilestone> List()
    {
        var outValue = new List<RetroPayMilestone>();
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
public sealed class CachedRetroPayMilestoneStore(MemoryRetroPayMilestoneStore inner, RetroPayMilestoneCache cache, int hits, int misses) : IRetroPayMilestoneStore
{
    public MemoryRetroPayMilestoneStore Inner { get; set; } = inner;
    public RetroPayMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayMilestone? Find(string code)
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

    public void Save(RetroPayMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay milestones. Empty fields match all.</summary>
public sealed record RetroPayMilestoneQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<RetroPayMilestone> Apply(List<RetroPayMilestone> items)
    {
        var matched = Collections.Filter(items, (RetroPayMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay milestone store.</summary>
public static class RetroPayMilestoneStore
{
    public static IRetroPayMilestoneStore NewMemoryRetroPayMilestoneStore()
    {
        return new MemoryRetroPayMilestoneStore(new Dictionary<string, RetroPayMilestone>(), new List<string>());
    }

    public static IRetroPayMilestoneStore NewCachedRetroPayMilestoneStore(MemoryRetroPayMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayMilestoneStore(inner, RetroPayMilestoneCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayMilestone> FindRetroPayMilestones(IRetroPayMilestoneStore store, RetroPayMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
