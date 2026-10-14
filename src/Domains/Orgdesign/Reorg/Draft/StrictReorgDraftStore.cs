namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict reorg drafts by code.</summary>
public interface IStrictReorgDraftStore
{
    StrictReorgDraft? Find(string code);
    void Save(StrictReorgDraft item);
    bool Remove(string code);
    List<StrictReorgDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReorgDraftStore(Dictionary<string, StrictReorgDraft> items, List<string> order) : IStrictReorgDraftStore
{
    public Dictionary<string, StrictReorgDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReorgDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReorgDraft item)
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

    public List<StrictReorgDraft> List()
    {
        var outValue = new List<StrictReorgDraft>();
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
public sealed class CachedStrictReorgDraftStore(MemoryStrictReorgDraftStore inner, StrictReorgDraftCache cache, int hits, int misses) : IStrictReorgDraftStore
{
    public MemoryStrictReorgDraftStore Inner { get; set; } = inner;
    public StrictReorgDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReorgDraft? Find(string code)
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

    public void Save(StrictReorgDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReorgDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict reorg drafts. Empty fields match all.</summary>
public sealed record StrictReorgDraftQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReorgDraft item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReorgDraft> Apply(List<StrictReorgDraft> items)
    {
        var matched = Collections.Filter(items, (StrictReorgDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict reorg draft store.</summary>
public static class StrictReorgDraftStore
{
    public static IStrictReorgDraftStore NewMemoryStrictReorgDraftStore()
    {
        return new MemoryStrictReorgDraftStore(new Dictionary<string, StrictReorgDraft>(), new List<string>());
    }

    public static IStrictReorgDraftStore NewCachedStrictReorgDraftStore(MemoryStrictReorgDraftStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReorgDraftStore(inner, StrictReorgDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictReorgDraft> FindStrictReorgDrafts(IStrictReorgDraftStore store, StrictReorgDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
