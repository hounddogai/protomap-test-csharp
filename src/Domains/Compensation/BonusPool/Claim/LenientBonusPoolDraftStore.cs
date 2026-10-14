namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient bonus pool drafts by code.</summary>
public interface ILenientBonusPoolDraftStore
{
    LenientBonusPoolDraft? Find(string code);
    void Save(LenientBonusPoolDraft item);
    bool Remove(string code);
    List<LenientBonusPoolDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBonusPoolDraftStore(Dictionary<string, LenientBonusPoolDraft> items, List<string> order) : ILenientBonusPoolDraftStore
{
    public Dictionary<string, LenientBonusPoolDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBonusPoolDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBonusPoolDraft item)
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

    public List<LenientBonusPoolDraft> List()
    {
        var outValue = new List<LenientBonusPoolDraft>();
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
public sealed class CachedLenientBonusPoolDraftStore(MemoryLenientBonusPoolDraftStore inner, LenientBonusPoolDraftCache cache, int hits, int misses) : ILenientBonusPoolDraftStore
{
    public MemoryLenientBonusPoolDraftStore Inner { get; set; } = inner;
    public LenientBonusPoolDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBonusPoolDraft? Find(string code)
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

    public void Save(LenientBonusPoolDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBonusPoolDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient bonus pool drafts. Empty fields match all.</summary>
public sealed record LenientBonusPoolDraftQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBonusPoolDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LenientBonusPoolDraft> Apply(List<LenientBonusPoolDraft> items)
    {
        var matched = Collections.Filter(items, (LenientBonusPoolDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient bonus pool draft store.</summary>
public static class LenientBonusPoolDraftStore
{
    public static ILenientBonusPoolDraftStore NewMemoryLenientBonusPoolDraftStore()
    {
        return new MemoryLenientBonusPoolDraftStore(
            new Dictionary<string, LenientBonusPoolDraft>(),
            new List<string>());
    }

    public static ILenientBonusPoolDraftStore NewCachedLenientBonusPoolDraftStore(MemoryLenientBonusPoolDraftStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBonusPoolDraftStore(inner, LenientBonusPoolDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientBonusPoolDraft> FindLenientBonusPoolDrafts(ILenientBonusPoolDraftStore store, LenientBonusPoolDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
