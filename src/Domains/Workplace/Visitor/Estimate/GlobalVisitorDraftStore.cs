namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global visitor drafts by code.</summary>
public interface IGlobalVisitorDraftStore
{
    GlobalVisitorDraft? Find(string code);
    void Save(GlobalVisitorDraft item);
    bool Remove(string code);
    List<GlobalVisitorDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalVisitorDraftStore(Dictionary<string, GlobalVisitorDraft> items, List<string> order) : IGlobalVisitorDraftStore
{
    public Dictionary<string, GlobalVisitorDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalVisitorDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalVisitorDraft item)
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

    public List<GlobalVisitorDraft> List()
    {
        var outValue = new List<GlobalVisitorDraft>();
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
public sealed class CachedGlobalVisitorDraftStore(MemoryGlobalVisitorDraftStore inner, GlobalVisitorDraftCache cache, int hits, int misses) : IGlobalVisitorDraftStore
{
    public MemoryGlobalVisitorDraftStore Inner { get; set; } = inner;
    public GlobalVisitorDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalVisitorDraft? Find(string code)
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

    public void Save(GlobalVisitorDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalVisitorDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global visitor drafts. Empty fields match all.</summary>
public sealed record GlobalVisitorDraftQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalVisitorDraft item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalVisitorDraft> Apply(List<GlobalVisitorDraft> items)
    {
        var matched = Collections.Filter(items, (GlobalVisitorDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global visitor draft store.</summary>
public static class GlobalVisitorDraftStore
{
    public static IGlobalVisitorDraftStore NewMemoryGlobalVisitorDraftStore()
    {
        return new MemoryGlobalVisitorDraftStore(new Dictionary<string, GlobalVisitorDraft>(), new List<string>());
    }

    public static IGlobalVisitorDraftStore NewCachedGlobalVisitorDraftStore(MemoryGlobalVisitorDraftStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalVisitorDraftStore(inner, GlobalVisitorDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalVisitorDraft> FindGlobalVisitorDrafts(IGlobalVisitorDraftStore store, GlobalVisitorDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
