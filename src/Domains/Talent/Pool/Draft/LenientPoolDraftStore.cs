namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pool drafts by code.</summary>
public interface ILenientPoolDraftStore
{
    LenientPoolDraft? Find(string code);
    void Save(LenientPoolDraft item);
    bool Remove(string code);
    List<LenientPoolDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPoolDraftStore(Dictionary<string, LenientPoolDraft> items, List<string> order) : ILenientPoolDraftStore
{
    public Dictionary<string, LenientPoolDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPoolDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPoolDraft item)
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

    public List<LenientPoolDraft> List()
    {
        var outValue = new List<LenientPoolDraft>();
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
public sealed class CachedLenientPoolDraftStore(MemoryLenientPoolDraftStore inner, LenientPoolDraftCache cache, int hits, int misses) : ILenientPoolDraftStore
{
    public MemoryLenientPoolDraftStore Inner { get; set; } = inner;
    public LenientPoolDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPoolDraft? Find(string code)
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

    public void Save(LenientPoolDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPoolDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pool drafts. Empty fields match all.</summary>
public sealed record LenientPoolDraftQuery(string Channel, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPoolDraft item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPoolDraft> Apply(List<LenientPoolDraft> items)
    {
        var matched = Collections.Filter(items, (LenientPoolDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pool draft store.</summary>
public static class LenientPoolDraftStore
{
    public static ILenientPoolDraftStore NewMemoryLenientPoolDraftStore()
    {
        return new MemoryLenientPoolDraftStore(new Dictionary<string, LenientPoolDraft>(), new List<string>());
    }

    public static ILenientPoolDraftStore NewCachedLenientPoolDraftStore(MemoryLenientPoolDraftStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPoolDraftStore(inner, LenientPoolDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientPoolDraft> FindLenientPoolDrafts(ILenientPoolDraftStore store, LenientPoolDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
