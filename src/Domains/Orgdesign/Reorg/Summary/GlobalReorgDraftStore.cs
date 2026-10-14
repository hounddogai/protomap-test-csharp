namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global reorg drafts by code.</summary>
public interface IGlobalReorgDraftStore
{
    GlobalReorgDraft? Find(string code);
    void Save(GlobalReorgDraft item);
    bool Remove(string code);
    List<GlobalReorgDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalReorgDraftStore(Dictionary<string, GlobalReorgDraft> items, List<string> order) : IGlobalReorgDraftStore
{
    public Dictionary<string, GlobalReorgDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalReorgDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalReorgDraft item)
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

    public List<GlobalReorgDraft> List()
    {
        var outValue = new List<GlobalReorgDraft>();
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
public sealed class CachedGlobalReorgDraftStore(MemoryGlobalReorgDraftStore inner, GlobalReorgDraftCache cache, int hits, int misses) : IGlobalReorgDraftStore
{
    public MemoryGlobalReorgDraftStore Inner { get; set; } = inner;
    public GlobalReorgDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalReorgDraft? Find(string code)
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

    public void Save(GlobalReorgDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalReorgDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global reorg drafts. Empty fields match all.</summary>
public sealed record GlobalReorgDraftQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalReorgDraft item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalReorgDraft> Apply(List<GlobalReorgDraft> items)
    {
        var matched = Collections.Filter(items, (GlobalReorgDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global reorg draft store.</summary>
public static class GlobalReorgDraftStore
{
    public static IGlobalReorgDraftStore NewMemoryGlobalReorgDraftStore()
    {
        return new MemoryGlobalReorgDraftStore(new Dictionary<string, GlobalReorgDraft>(), new List<string>());
    }

    public static IGlobalReorgDraftStore NewCachedGlobalReorgDraftStore(MemoryGlobalReorgDraftStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalReorgDraftStore(inner, GlobalReorgDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalReorgDraft> FindGlobalReorgDrafts(IGlobalReorgDraftStore store, GlobalReorgDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
