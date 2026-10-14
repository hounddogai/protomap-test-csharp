namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount drafts by code.</summary>
public interface IHeadcountDraftStore
{
    HeadcountDraft? Find(string code);
    void Save(HeadcountDraft item);
    bool Remove(string code);
    List<HeadcountDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountDraftStore(Dictionary<string, HeadcountDraft> items, List<string> order) : IHeadcountDraftStore
{
    public Dictionary<string, HeadcountDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountDraft item)
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

    public List<HeadcountDraft> List()
    {
        var outValue = new List<HeadcountDraft>();
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
public sealed class CachedHeadcountDraftStore(MemoryHeadcountDraftStore inner, HeadcountDraftCache cache, int hits, int misses) : IHeadcountDraftStore
{
    public MemoryHeadcountDraftStore Inner { get; set; } = inner;
    public HeadcountDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountDraft? Find(string code)
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

    public void Save(HeadcountDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount drafts. Empty fields match all.</summary>
public sealed record HeadcountDraftQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountDraft> Apply(List<HeadcountDraft> items)
    {
        var matched = Collections.Filter(items, (HeadcountDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount draft store.</summary>
public static class HeadcountDraftStore
{
    public static IHeadcountDraftStore NewMemoryHeadcountDraftStore()
    {
        return new MemoryHeadcountDraftStore(new Dictionary<string, HeadcountDraft>(), new List<string>());
    }

    public static IHeadcountDraftStore NewCachedHeadcountDraftStore(MemoryHeadcountDraftStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountDraftStore(inner, HeadcountDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountDraft> FindHeadcountDrafts(IHeadcountDraftStore store, HeadcountDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
