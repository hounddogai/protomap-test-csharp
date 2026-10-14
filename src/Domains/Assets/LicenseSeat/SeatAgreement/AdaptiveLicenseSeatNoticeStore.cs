namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive license seat notices by code.</summary>
public interface IAdaptiveLicenseSeatNoticeStore
{
    AdaptiveLicenseSeatNotice? Find(string code);
    void Save(AdaptiveLicenseSeatNotice item);
    bool Remove(string code);
    List<AdaptiveLicenseSeatNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveLicenseSeatNoticeStore(Dictionary<string, AdaptiveLicenseSeatNotice> items, List<string> order) : IAdaptiveLicenseSeatNoticeStore
{
    public Dictionary<string, AdaptiveLicenseSeatNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveLicenseSeatNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveLicenseSeatNotice item)
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

    public List<AdaptiveLicenseSeatNotice> List()
    {
        var outValue = new List<AdaptiveLicenseSeatNotice>();
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
public sealed class CachedAdaptiveLicenseSeatNoticeStore(MemoryAdaptiveLicenseSeatNoticeStore inner, AdaptiveLicenseSeatNoticeCache cache, int hits, int misses) : IAdaptiveLicenseSeatNoticeStore
{
    public MemoryAdaptiveLicenseSeatNoticeStore Inner { get; set; } = inner;
    public AdaptiveLicenseSeatNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveLicenseSeatNotice? Find(string code)
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

    public void Save(AdaptiveLicenseSeatNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveLicenseSeatNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive license seat notices. Empty fields match all.</summary>
public sealed record AdaptiveLicenseSeatNoticeQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveLicenseSeatNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveLicenseSeatNotice> Apply(List<AdaptiveLicenseSeatNotice> items)
    {
        var matched = Collections.Filter(items, (AdaptiveLicenseSeatNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive license seat notice store.</summary>
public static class AdaptiveLicenseSeatNoticeStore
{
    public static IAdaptiveLicenseSeatNoticeStore NewMemoryAdaptiveLicenseSeatNoticeStore()
    {
        return new MemoryAdaptiveLicenseSeatNoticeStore(
            new Dictionary<string, AdaptiveLicenseSeatNotice>(),
            new List<string>());
    }

    public static IAdaptiveLicenseSeatNoticeStore NewCachedAdaptiveLicenseSeatNoticeStore(MemoryAdaptiveLicenseSeatNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveLicenseSeatNoticeStore(
            inner,
            AdaptiveLicenseSeatNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveLicenseSeatNotice> FindAdaptiveLicenseSeatNotices(IAdaptiveLicenseSeatNoticeStore store, AdaptiveLicenseSeatNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
