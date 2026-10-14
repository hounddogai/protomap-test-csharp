namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped action plan drafts by code.</summary>
public interface ICappedActionPlanDraftStore
{
    CappedActionPlanDraft? Find(string code);
    void Save(CappedActionPlanDraft item);
    bool Remove(string code);
    List<CappedActionPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedActionPlanDraftStore(Dictionary<string, CappedActionPlanDraft> items, List<string> order) : ICappedActionPlanDraftStore
{
    public Dictionary<string, CappedActionPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedActionPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedActionPlanDraft item)
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

    public List<CappedActionPlanDraft> List()
    {
        var outValue = new List<CappedActionPlanDraft>();
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
public sealed class CachedCappedActionPlanDraftStore(MemoryCappedActionPlanDraftStore inner, CappedActionPlanDraftCache cache, int hits, int misses) : ICappedActionPlanDraftStore
{
    public MemoryCappedActionPlanDraftStore Inner { get; set; } = inner;
    public CappedActionPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedActionPlanDraft? Find(string code)
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

    public void Save(CappedActionPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedActionPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped action plan drafts. Empty fields match all.</summary>
public sealed record CappedActionPlanDraftQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedActionPlanDraft item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<CappedActionPlanDraft> Apply(List<CappedActionPlanDraft> items)
    {
        var matched = Collections.Filter(items, (CappedActionPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped action plan draft store.</summary>
public static class CappedActionPlanDraftStore
{
    public static ICappedActionPlanDraftStore NewMemoryCappedActionPlanDraftStore()
    {
        return new MemoryCappedActionPlanDraftStore(
            new Dictionary<string, CappedActionPlanDraft>(),
            new List<string>());
    }

    public static ICappedActionPlanDraftStore NewCachedCappedActionPlanDraftStore(MemoryCappedActionPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedCappedActionPlanDraftStore(inner, CappedActionPlanDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedActionPlanDraft> FindCappedActionPlanDrafts(ICappedActionPlanDraftStore store, CappedActionPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
