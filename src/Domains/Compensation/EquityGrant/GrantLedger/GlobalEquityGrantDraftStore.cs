namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global equity grant drafts by code.</summary>
public interface IGlobalEquityGrantDraftStore
{
    GlobalEquityGrantDraft? Find(string code);
    void Save(GlobalEquityGrantDraft item);
    bool Remove(string code);
    List<GlobalEquityGrantDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEquityGrantDraftStore(Dictionary<string, GlobalEquityGrantDraft> items, List<string> order) : IGlobalEquityGrantDraftStore
{
    public Dictionary<string, GlobalEquityGrantDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEquityGrantDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEquityGrantDraft item)
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

    public List<GlobalEquityGrantDraft> List()
    {
        var outValue = new List<GlobalEquityGrantDraft>();
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
public sealed class CachedGlobalEquityGrantDraftStore(MemoryGlobalEquityGrantDraftStore inner, GlobalEquityGrantDraftCache cache, int hits, int misses) : IGlobalEquityGrantDraftStore
{
    public MemoryGlobalEquityGrantDraftStore Inner { get; set; } = inner;
    public GlobalEquityGrantDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEquityGrantDraft? Find(string code)
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

    public void Save(GlobalEquityGrantDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEquityGrantDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global equity grant drafts. Empty fields match all.</summary>
public sealed record GlobalEquityGrantDraftQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEquityGrantDraft item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalEquityGrantDraft> Apply(List<GlobalEquityGrantDraft> items)
    {
        var matched = Collections.Filter(items, (GlobalEquityGrantDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global equity grant draft store.</summary>
public static class GlobalEquityGrantDraftStore
{
    public static IGlobalEquityGrantDraftStore NewMemoryGlobalEquityGrantDraftStore()
    {
        return new MemoryGlobalEquityGrantDraftStore(
            new Dictionary<string, GlobalEquityGrantDraft>(),
            new List<string>());
    }

    public static IGlobalEquityGrantDraftStore NewCachedGlobalEquityGrantDraftStore(MemoryGlobalEquityGrantDraftStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEquityGrantDraftStore(
            inner,
            GlobalEquityGrantDraftCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEquityGrantDraft> FindGlobalEquityGrantDrafts(IGlobalEquityGrantDraftStore store, GlobalEquityGrantDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
