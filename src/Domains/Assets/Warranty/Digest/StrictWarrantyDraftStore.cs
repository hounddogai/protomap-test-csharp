namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict warranty drafts by code.</summary>
public interface IStrictWarrantyDraftStore
{
    StrictWarrantyDraft? Find(string code);
    void Save(StrictWarrantyDraft item);
    bool Remove(string code);
    List<StrictWarrantyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictWarrantyDraftStore(Dictionary<string, StrictWarrantyDraft> items, List<string> order) : IStrictWarrantyDraftStore
{
    public Dictionary<string, StrictWarrantyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictWarrantyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictWarrantyDraft item)
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

    public List<StrictWarrantyDraft> List()
    {
        var outValue = new List<StrictWarrantyDraft>();
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
public sealed class CachedStrictWarrantyDraftStore(MemoryStrictWarrantyDraftStore inner, StrictWarrantyDraftCache cache, int hits, int misses) : IStrictWarrantyDraftStore
{
    public MemoryStrictWarrantyDraftStore Inner { get; set; } = inner;
    public StrictWarrantyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictWarrantyDraft? Find(string code)
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

    public void Save(StrictWarrantyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictWarrantyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict warranty drafts. Empty fields match all.</summary>
public sealed record StrictWarrantyDraftQuery(string Source, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictWarrantyDraft item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictWarrantyDraft> Apply(List<StrictWarrantyDraft> items)
    {
        var matched = Collections.Filter(items, (StrictWarrantyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict warranty draft store.</summary>
public static class StrictWarrantyDraftStore
{
    public static IStrictWarrantyDraftStore NewMemoryStrictWarrantyDraftStore()
    {
        return new MemoryStrictWarrantyDraftStore(new Dictionary<string, StrictWarrantyDraft>(), new List<string>());
    }

    public static IStrictWarrantyDraftStore NewCachedStrictWarrantyDraftStore(MemoryStrictWarrantyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedStrictWarrantyDraftStore(inner, StrictWarrantyDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictWarrantyDraft> FindStrictWarrantyDrafts(IStrictWarrantyDraftStore store, StrictWarrantyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
