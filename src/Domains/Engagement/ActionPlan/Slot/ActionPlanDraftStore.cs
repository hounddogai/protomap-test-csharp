namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan drafts by code.</summary>
public interface IActionPlanDraftStore
{
    ActionPlanDraft? Find(string code);
    void Save(ActionPlanDraft item);
    bool Remove(string code);
    List<ActionPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanDraftStore(Dictionary<string, ActionPlanDraft> items, List<string> order) : IActionPlanDraftStore
{
    public Dictionary<string, ActionPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanDraft item)
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

    public List<ActionPlanDraft> List()
    {
        var outValue = new List<ActionPlanDraft>();
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
public sealed class CachedActionPlanDraftStore(MemoryActionPlanDraftStore inner, ActionPlanDraftCache cache, int hits, int misses) : IActionPlanDraftStore
{
    public MemoryActionPlanDraftStore Inner { get; set; } = inner;
    public ActionPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanDraft? Find(string code)
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

    public void Save(ActionPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan drafts. Empty fields match all.</summary>
public sealed record ActionPlanDraftQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanDraft item)
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

    public List<ActionPlanDraft> Apply(List<ActionPlanDraft> items)
    {
        var matched = Collections.Filter(items, (ActionPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan draft store.</summary>
public static class ActionPlanDraftStore
{
    public static IActionPlanDraftStore NewMemoryActionPlanDraftStore()
    {
        return new MemoryActionPlanDraftStore(new Dictionary<string, ActionPlanDraft>(), new List<string>());
    }

    public static IActionPlanDraftStore NewCachedActionPlanDraftStore(MemoryActionPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanDraftStore(inner, ActionPlanDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ActionPlanDraft> FindActionPlanDrafts(IActionPlanDraftStore store, ActionPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
