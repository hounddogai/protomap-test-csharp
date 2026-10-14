namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default action plan drafts by code.</summary>
public interface IDefaultActionPlanDraftStore
{
    DefaultActionPlanDraft? Find(string code);
    void Save(DefaultActionPlanDraft item);
    bool Remove(string code);
    List<DefaultActionPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultActionPlanDraftStore(Dictionary<string, DefaultActionPlanDraft> items, List<string> order) : IDefaultActionPlanDraftStore
{
    public Dictionary<string, DefaultActionPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultActionPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultActionPlanDraft item)
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

    public List<DefaultActionPlanDraft> List()
    {
        var outValue = new List<DefaultActionPlanDraft>();
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
public sealed class CachedDefaultActionPlanDraftStore(MemoryDefaultActionPlanDraftStore inner, DefaultActionPlanDraftCache cache, int hits, int misses) : IDefaultActionPlanDraftStore
{
    public MemoryDefaultActionPlanDraftStore Inner { get; set; } = inner;
    public DefaultActionPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultActionPlanDraft? Find(string code)
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

    public void Save(DefaultActionPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultActionPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default action plan drafts. Empty fields match all.</summary>
public sealed record DefaultActionPlanDraftQuery(string Channel, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultActionPlanDraft item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultActionPlanDraft> Apply(List<DefaultActionPlanDraft> items)
    {
        var matched = Collections.Filter(items, (DefaultActionPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default action plan draft store.</summary>
public static class DefaultActionPlanDraftStore
{
    public static IDefaultActionPlanDraftStore NewMemoryDefaultActionPlanDraftStore()
    {
        return new MemoryDefaultActionPlanDraftStore(
            new Dictionary<string, DefaultActionPlanDraft>(),
            new List<string>());
    }

    public static IDefaultActionPlanDraftStore NewCachedDefaultActionPlanDraftStore(MemoryDefaultActionPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultActionPlanDraftStore(
            inner,
            DefaultActionPlanDraftCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultActionPlanDraft> FindDefaultActionPlanDrafts(IDefaultActionPlanDraftStore store, DefaultActionPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
