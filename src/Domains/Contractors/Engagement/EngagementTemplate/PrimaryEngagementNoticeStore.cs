namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary engagement notices by code.</summary>
public interface IPrimaryEngagementNoticeStore
{
    PrimaryEngagementNotice? Find(string code);
    void Save(PrimaryEngagementNotice item);
    bool Remove(string code);
    List<PrimaryEngagementNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEngagementNoticeStore(Dictionary<string, PrimaryEngagementNotice> items, List<string> order) : IPrimaryEngagementNoticeStore
{
    public Dictionary<string, PrimaryEngagementNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEngagementNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEngagementNotice item)
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

    public List<PrimaryEngagementNotice> List()
    {
        var outValue = new List<PrimaryEngagementNotice>();
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
public sealed class CachedPrimaryEngagementNoticeStore(MemoryPrimaryEngagementNoticeStore inner, PrimaryEngagementNoticeCache cache, int hits, int misses) : IPrimaryEngagementNoticeStore
{
    public MemoryPrimaryEngagementNoticeStore Inner { get; set; } = inner;
    public PrimaryEngagementNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEngagementNotice? Find(string code)
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

    public void Save(PrimaryEngagementNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEngagementNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary engagement notices. Empty fields match all.</summary>
public sealed record PrimaryEngagementNoticeQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEngagementNotice item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<PrimaryEngagementNotice> Apply(List<PrimaryEngagementNotice> items)
    {
        var matched = Collections.Filter(items, (PrimaryEngagementNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary engagement notice store.</summary>
public static class PrimaryEngagementNoticeStore
{
    public static IPrimaryEngagementNoticeStore NewMemoryPrimaryEngagementNoticeStore()
    {
        return new MemoryPrimaryEngagementNoticeStore(
            new Dictionary<string, PrimaryEngagementNotice>(),
            new List<string>());
    }

    public static IPrimaryEngagementNoticeStore NewCachedPrimaryEngagementNoticeStore(MemoryPrimaryEngagementNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEngagementNoticeStore(
            inner,
            PrimaryEngagementNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEngagementNotice> FindPrimaryEngagementNotices(IPrimaryEngagementNoticeStore store, PrimaryEngagementNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
