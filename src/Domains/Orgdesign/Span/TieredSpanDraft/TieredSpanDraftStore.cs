namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered span drafts by code.</summary>
public interface ITieredSpanDraftStore
{
    TieredSpanDraft? Find(string code);
    void Save(TieredSpanDraft item);
    bool Remove(string code);
    List<TieredSpanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSpanDraftStore(Dictionary<string, TieredSpanDraft> items, List<string> order) : ITieredSpanDraftStore
{
    public Dictionary<string, TieredSpanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSpanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSpanDraft item)
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

    public List<TieredSpanDraft> List()
    {
        var outValue = new List<TieredSpanDraft>();
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
public sealed class CachedTieredSpanDraftStore(MemoryTieredSpanDraftStore inner, TieredSpanDraftCache cache, int hits, int misses) : ITieredSpanDraftStore
{
    public MemoryTieredSpanDraftStore Inner { get; set; } = inner;
    public TieredSpanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSpanDraft? Find(string code)
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

    public void Save(TieredSpanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSpanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered span drafts. Empty fields match all.</summary>
public sealed record TieredSpanDraftQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSpanDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSpanDraft> Apply(List<TieredSpanDraft> items)
    {
        var matched = Collections.Filter(items, (TieredSpanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered span draft store.</summary>
public static class TieredSpanDraftStore
{
    public static ITieredSpanDraftStore NewMemoryTieredSpanDraftStore()
    {
        return new MemoryTieredSpanDraftStore(new Dictionary<string, TieredSpanDraft>(), new List<string>());
    }

    public static ITieredSpanDraftStore NewCachedTieredSpanDraftStore(MemoryTieredSpanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSpanDraftStore(inner, TieredSpanDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredSpanDraft> FindTieredSpanDrafts(ITieredSpanDraftStore store, TieredSpanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
