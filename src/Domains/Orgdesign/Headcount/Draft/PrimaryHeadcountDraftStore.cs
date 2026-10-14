namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary headcount drafts by code.</summary>
public interface IPrimaryHeadcountDraftStore
{
    PrimaryHeadcountDraft? Find(string code);
    void Save(PrimaryHeadcountDraft item);
    bool Remove(string code);
    List<PrimaryHeadcountDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryHeadcountDraftStore(Dictionary<string, PrimaryHeadcountDraft> items, List<string> order) : IPrimaryHeadcountDraftStore
{
    public Dictionary<string, PrimaryHeadcountDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryHeadcountDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryHeadcountDraft item)
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

    public List<PrimaryHeadcountDraft> List()
    {
        var outValue = new List<PrimaryHeadcountDraft>();
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
public sealed class CachedPrimaryHeadcountDraftStore(MemoryPrimaryHeadcountDraftStore inner, PrimaryHeadcountDraftCache cache, int hits, int misses) : IPrimaryHeadcountDraftStore
{
    public MemoryPrimaryHeadcountDraftStore Inner { get; set; } = inner;
    public PrimaryHeadcountDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryHeadcountDraft? Find(string code)
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

    public void Save(PrimaryHeadcountDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryHeadcountDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary headcount drafts. Empty fields match all.</summary>
public sealed record PrimaryHeadcountDraftQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryHeadcountDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PrimaryHeadcountDraft> Apply(List<PrimaryHeadcountDraft> items)
    {
        var matched = Collections.Filter(items, (PrimaryHeadcountDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary headcount draft store.</summary>
public static class PrimaryHeadcountDraftStore
{
    public static IPrimaryHeadcountDraftStore NewMemoryPrimaryHeadcountDraftStore()
    {
        return new MemoryPrimaryHeadcountDraftStore(
            new Dictionary<string, PrimaryHeadcountDraft>(),
            new List<string>());
    }

    public static IPrimaryHeadcountDraftStore NewCachedPrimaryHeadcountDraftStore(MemoryPrimaryHeadcountDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryHeadcountDraftStore(inner, PrimaryHeadcountDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryHeadcountDraft> FindPrimaryHeadcountDrafts(IPrimaryHeadcountDraftStore store, PrimaryHeadcountDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
