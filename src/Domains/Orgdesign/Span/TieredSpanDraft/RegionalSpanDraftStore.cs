namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional span drafts by code.</summary>
public interface IRegionalSpanDraftStore
{
    RegionalSpanDraft? Find(string code);
    void Save(RegionalSpanDraft item);
    bool Remove(string code);
    List<RegionalSpanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSpanDraftStore(Dictionary<string, RegionalSpanDraft> items, List<string> order) : IRegionalSpanDraftStore
{
    public Dictionary<string, RegionalSpanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSpanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSpanDraft item)
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

    public List<RegionalSpanDraft> List()
    {
        var outValue = new List<RegionalSpanDraft>();
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
public sealed class CachedRegionalSpanDraftStore(MemoryRegionalSpanDraftStore inner, RegionalSpanDraftCache cache, int hits, int misses) : IRegionalSpanDraftStore
{
    public MemoryRegionalSpanDraftStore Inner { get; set; } = inner;
    public RegionalSpanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSpanDraft? Find(string code)
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

    public void Save(RegionalSpanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSpanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional span drafts. Empty fields match all.</summary>
public sealed record RegionalSpanDraftQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSpanDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSpanDraft> Apply(List<RegionalSpanDraft> items)
    {
        var matched = Collections.Filter(items, (RegionalSpanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional span draft store.</summary>
public static class RegionalSpanDraftStore
{
    public static IRegionalSpanDraftStore NewMemoryRegionalSpanDraftStore()
    {
        return new MemoryRegionalSpanDraftStore(new Dictionary<string, RegionalSpanDraft>(), new List<string>());
    }

    public static IRegionalSpanDraftStore NewCachedRegionalSpanDraftStore(MemoryRegionalSpanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSpanDraftStore(inner, RegionalSpanDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RegionalSpanDraft> FindRegionalSpanDrafts(IRegionalSpanDraftStore store, RegionalSpanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
