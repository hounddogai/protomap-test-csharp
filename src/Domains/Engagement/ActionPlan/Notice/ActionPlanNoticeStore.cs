namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan notices by code.</summary>
public interface IActionPlanNoticeStore
{
    ActionPlanNotice? Find(string code);
    void Save(ActionPlanNotice item);
    bool Remove(string code);
    List<ActionPlanNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanNoticeStore(Dictionary<string, ActionPlanNotice> items, List<string> order) : IActionPlanNoticeStore
{
    public Dictionary<string, ActionPlanNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanNotice item)
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

    public List<ActionPlanNotice> List()
    {
        var outValue = new List<ActionPlanNotice>();
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
public sealed class CachedActionPlanNoticeStore(MemoryActionPlanNoticeStore inner, ActionPlanNoticeCache cache, int hits, int misses) : IActionPlanNoticeStore
{
    public MemoryActionPlanNoticeStore Inner { get; set; } = inner;
    public ActionPlanNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanNotice? Find(string code)
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

    public void Save(ActionPlanNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan notices. Empty fields match all.</summary>
public sealed record ActionPlanNoticeQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanNotice> Apply(List<ActionPlanNotice> items)
    {
        var matched = Collections.Filter(items, (ActionPlanNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan notice store.</summary>
public static class ActionPlanNoticeStore
{
    public static IActionPlanNoticeStore NewMemoryActionPlanNoticeStore()
    {
        return new MemoryActionPlanNoticeStore(new Dictionary<string, ActionPlanNotice>(), new List<string>());
    }

    public static IActionPlanNoticeStore NewCachedActionPlanNoticeStore(MemoryActionPlanNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanNoticeStore(inner, ActionPlanNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanNotice> FindActionPlanNotices(IActionPlanNoticeStore store, ActionPlanNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
