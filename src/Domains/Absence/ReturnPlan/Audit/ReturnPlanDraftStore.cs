namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan drafts by code.</summary>
public interface IReturnPlanDraftStore
{
    ReturnPlanDraft? Find(string code);
    void Save(ReturnPlanDraft item);
    bool Remove(string code);
    List<ReturnPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanDraftStore(Dictionary<string, ReturnPlanDraft> items, List<string> order) : IReturnPlanDraftStore
{
    public Dictionary<string, ReturnPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanDraft item)
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

    public List<ReturnPlanDraft> List()
    {
        var outValue = new List<ReturnPlanDraft>();
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
public sealed class CachedReturnPlanDraftStore(MemoryReturnPlanDraftStore inner, ReturnPlanDraftCache cache, int hits, int misses) : IReturnPlanDraftStore
{
    public MemoryReturnPlanDraftStore Inner { get; set; } = inner;
    public ReturnPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanDraft? Find(string code)
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

    public void Save(ReturnPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan drafts. Empty fields match all.</summary>
public sealed record ReturnPlanDraftQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ReturnPlanDraft> Apply(List<ReturnPlanDraft> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan draft store.</summary>
public static class ReturnPlanDraftStore
{
    public static IReturnPlanDraftStore NewMemoryReturnPlanDraftStore()
    {
        return new MemoryReturnPlanDraftStore(new Dictionary<string, ReturnPlanDraft>(), new List<string>());
    }

    public static IReturnPlanDraftStore NewCachedReturnPlanDraftStore(MemoryReturnPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanDraftStore(inner, ReturnPlanDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanDraft> FindReturnPlanDrafts(IReturnPlanDraftStore store, ReturnPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
