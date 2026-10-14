namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive career path notices by code.</summary>
public interface IAdaptiveCareerPathNoticeStore
{
    AdaptiveCareerPathNotice? Find(string code);
    void Save(AdaptiveCareerPathNotice item);
    bool Remove(string code);
    List<AdaptiveCareerPathNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCareerPathNoticeStore(Dictionary<string, AdaptiveCareerPathNotice> items, List<string> order) : IAdaptiveCareerPathNoticeStore
{
    public Dictionary<string, AdaptiveCareerPathNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCareerPathNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCareerPathNotice item)
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

    public List<AdaptiveCareerPathNotice> List()
    {
        var outValue = new List<AdaptiveCareerPathNotice>();
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
public sealed class CachedAdaptiveCareerPathNoticeStore(MemoryAdaptiveCareerPathNoticeStore inner, AdaptiveCareerPathNoticeCache cache, int hits, int misses) : IAdaptiveCareerPathNoticeStore
{
    public MemoryAdaptiveCareerPathNoticeStore Inner { get; set; } = inner;
    public AdaptiveCareerPathNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCareerPathNotice? Find(string code)
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

    public void Save(AdaptiveCareerPathNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCareerPathNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive career path notices. Empty fields match all.</summary>
public sealed record AdaptiveCareerPathNoticeQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCareerPathNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCareerPathNotice> Apply(List<AdaptiveCareerPathNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCareerPathNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive career path notice store.</summary>
public static class AdaptiveCareerPathNoticeStore
{
    public static IAdaptiveCareerPathNoticeStore NewMemoryAdaptiveCareerPathNoticeStore()
    {
        return new MemoryAdaptiveCareerPathNoticeStore(
            new Dictionary<string, AdaptiveCareerPathNotice>(),
            new List<string>());
    }

    public static IAdaptiveCareerPathNoticeStore NewCachedAdaptiveCareerPathNoticeStore(MemoryAdaptiveCareerPathNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCareerPathNoticeStore(
            inner,
            AdaptiveCareerPathNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCareerPathNotice> FindAdaptiveCareerPathNotices(IAdaptiveCareerPathNoticeStore store, AdaptiveCareerPathNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
