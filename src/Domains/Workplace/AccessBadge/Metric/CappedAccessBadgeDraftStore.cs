namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped access badge drafts by code.</summary>
public interface ICappedAccessBadgeDraftStore
{
    CappedAccessBadgeDraft? Find(string code);
    void Save(CappedAccessBadgeDraft item);
    bool Remove(string code);
    List<CappedAccessBadgeDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAccessBadgeDraftStore(Dictionary<string, CappedAccessBadgeDraft> items, List<string> order) : ICappedAccessBadgeDraftStore
{
    public Dictionary<string, CappedAccessBadgeDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAccessBadgeDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAccessBadgeDraft item)
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

    public List<CappedAccessBadgeDraft> List()
    {
        var outValue = new List<CappedAccessBadgeDraft>();
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
public sealed class CachedCappedAccessBadgeDraftStore(MemoryCappedAccessBadgeDraftStore inner, CappedAccessBadgeDraftCache cache, int hits, int misses) : ICappedAccessBadgeDraftStore
{
    public MemoryCappedAccessBadgeDraftStore Inner { get; set; } = inner;
    public CappedAccessBadgeDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAccessBadgeDraft? Find(string code)
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

    public void Save(CappedAccessBadgeDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAccessBadgeDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped access badge drafts. Empty fields match all.</summary>
public sealed record CappedAccessBadgeDraftQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAccessBadgeDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAccessBadgeDraft> Apply(List<CappedAccessBadgeDraft> items)
    {
        var matched = Collections.Filter(items, (CappedAccessBadgeDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped access badge draft store.</summary>
public static class CappedAccessBadgeDraftStore
{
    public static ICappedAccessBadgeDraftStore NewMemoryCappedAccessBadgeDraftStore()
    {
        return new MemoryCappedAccessBadgeDraftStore(
            new Dictionary<string, CappedAccessBadgeDraft>(),
            new List<string>());
    }

    public static ICappedAccessBadgeDraftStore NewCachedCappedAccessBadgeDraftStore(MemoryCappedAccessBadgeDraftStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAccessBadgeDraftStore(
            inner,
            CappedAccessBadgeDraftCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedAccessBadgeDraft> FindCappedAccessBadgeDrafts(ICappedAccessBadgeDraftStore store, CappedAccessBadgeDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
