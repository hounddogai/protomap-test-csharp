namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pay band drafts by code.</summary>
public interface IRegionalPayBandDraftStore
{
    RegionalPayBandDraft? Find(string code);
    void Save(RegionalPayBandDraft item);
    bool Remove(string code);
    List<RegionalPayBandDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayBandDraftStore(Dictionary<string, RegionalPayBandDraft> items, List<string> order) : IRegionalPayBandDraftStore
{
    public Dictionary<string, RegionalPayBandDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayBandDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayBandDraft item)
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

    public List<RegionalPayBandDraft> List()
    {
        var outValue = new List<RegionalPayBandDraft>();
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
public sealed class CachedRegionalPayBandDraftStore(MemoryRegionalPayBandDraftStore inner, RegionalPayBandDraftCache cache, int hits, int misses) : IRegionalPayBandDraftStore
{
    public MemoryRegionalPayBandDraftStore Inner { get; set; } = inner;
    public RegionalPayBandDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayBandDraft? Find(string code)
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

    public void Save(RegionalPayBandDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayBandDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pay band drafts. Empty fields match all.</summary>
public sealed record RegionalPayBandDraftQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayBandDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RegionalPayBandDraft> Apply(List<RegionalPayBandDraft> items)
    {
        var matched = Collections.Filter(items, (RegionalPayBandDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pay band draft store.</summary>
public static class RegionalPayBandDraftStore
{
    public static IRegionalPayBandDraftStore NewMemoryRegionalPayBandDraftStore()
    {
        return new MemoryRegionalPayBandDraftStore(
            new Dictionary<string, RegionalPayBandDraft>(),
            new List<string>());
    }

    public static IRegionalPayBandDraftStore NewCachedRegionalPayBandDraftStore(MemoryRegionalPayBandDraftStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayBandDraftStore(inner, RegionalPayBandDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RegionalPayBandDraft> FindRegionalPayBandDrafts(IRegionalPayBandDraftStore store, RegionalPayBandDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
