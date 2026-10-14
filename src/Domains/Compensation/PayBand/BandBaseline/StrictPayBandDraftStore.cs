namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pay band drafts by code.</summary>
public interface IStrictPayBandDraftStore
{
    StrictPayBandDraft? Find(string code);
    void Save(StrictPayBandDraft item);
    bool Remove(string code);
    List<StrictPayBandDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPayBandDraftStore(Dictionary<string, StrictPayBandDraft> items, List<string> order) : IStrictPayBandDraftStore
{
    public Dictionary<string, StrictPayBandDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPayBandDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPayBandDraft item)
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

    public List<StrictPayBandDraft> List()
    {
        var outValue = new List<StrictPayBandDraft>();
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
public sealed class CachedStrictPayBandDraftStore(MemoryStrictPayBandDraftStore inner, StrictPayBandDraftCache cache, int hits, int misses) : IStrictPayBandDraftStore
{
    public MemoryStrictPayBandDraftStore Inner { get; set; } = inner;
    public StrictPayBandDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPayBandDraft? Find(string code)
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

    public void Save(StrictPayBandDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPayBandDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pay band drafts. Empty fields match all.</summary>
public sealed record StrictPayBandDraftQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPayBandDraft item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPayBandDraft> Apply(List<StrictPayBandDraft> items)
    {
        var matched = Collections.Filter(items, (StrictPayBandDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pay band draft store.</summary>
public static class StrictPayBandDraftStore
{
    public static IStrictPayBandDraftStore NewMemoryStrictPayBandDraftStore()
    {
        return new MemoryStrictPayBandDraftStore(new Dictionary<string, StrictPayBandDraft>(), new List<string>());
    }

    public static IStrictPayBandDraftStore NewCachedStrictPayBandDraftStore(MemoryStrictPayBandDraftStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPayBandDraftStore(inner, StrictPayBandDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictPayBandDraft> FindStrictPayBandDrafts(IStrictPayBandDraftStore store, StrictPayBandDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
