namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg drafts by code.</summary>
public interface IReorgDraftStore
{
    ReorgDraft? Find(string code);
    void Save(ReorgDraft item);
    bool Remove(string code);
    List<ReorgDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgDraftStore(Dictionary<string, ReorgDraft> items, List<string> order) : IReorgDraftStore
{
    public Dictionary<string, ReorgDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgDraft item)
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

    public List<ReorgDraft> List()
    {
        var outValue = new List<ReorgDraft>();
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
public sealed class CachedReorgDraftStore(MemoryReorgDraftStore inner, ReorgDraftCache cache, int hits, int misses) : IReorgDraftStore
{
    public MemoryReorgDraftStore Inner { get; set; } = inner;
    public ReorgDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgDraft? Find(string code)
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

    public void Save(ReorgDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg drafts. Empty fields match all.</summary>
public sealed record ReorgDraftQuery(string Region, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgDraft item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgDraft> Apply(List<ReorgDraft> items)
    {
        var matched = Collections.Filter(items, (ReorgDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg draft store.</summary>
public static class ReorgDraftStore
{
    public static IReorgDraftStore NewMemoryReorgDraftStore()
    {
        return new MemoryReorgDraftStore(new Dictionary<string, ReorgDraft>(), new List<string>());
    }

    public static IReorgDraftStore NewCachedReorgDraftStore(MemoryReorgDraftStore inner, int ttl, IClock clock)
    {
        return new CachedReorgDraftStore(inner, ReorgDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgDraft> FindReorgDrafts(IReorgDraftStore store, ReorgDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
