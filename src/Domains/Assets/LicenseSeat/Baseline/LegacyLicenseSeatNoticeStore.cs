namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy license seat notices by code.</summary>
public interface ILegacyLicenseSeatNoticeStore
{
    LegacyLicenseSeatNotice? Find(string code);
    void Save(LegacyLicenseSeatNotice item);
    bool Remove(string code);
    List<LegacyLicenseSeatNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLicenseSeatNoticeStore(Dictionary<string, LegacyLicenseSeatNotice> items, List<string> order) : ILegacyLicenseSeatNoticeStore
{
    public Dictionary<string, LegacyLicenseSeatNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLicenseSeatNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLicenseSeatNotice item)
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

    public List<LegacyLicenseSeatNotice> List()
    {
        var outValue = new List<LegacyLicenseSeatNotice>();
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
public sealed class CachedLegacyLicenseSeatNoticeStore(MemoryLegacyLicenseSeatNoticeStore inner, LegacyLicenseSeatNoticeCache cache, int hits, int misses) : ILegacyLicenseSeatNoticeStore
{
    public MemoryLegacyLicenseSeatNoticeStore Inner { get; set; } = inner;
    public LegacyLicenseSeatNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLicenseSeatNotice? Find(string code)
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

    public void Save(LegacyLicenseSeatNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLicenseSeatNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy license seat notices. Empty fields match all.</summary>
public sealed record LegacyLicenseSeatNoticeQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLicenseSeatNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LegacyLicenseSeatNotice> Apply(List<LegacyLicenseSeatNotice> items)
    {
        var matched = Collections.Filter(items, (LegacyLicenseSeatNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy license seat notice store.</summary>
public static class LegacyLicenseSeatNoticeStore
{
    public static ILegacyLicenseSeatNoticeStore NewMemoryLegacyLicenseSeatNoticeStore()
    {
        return new MemoryLegacyLicenseSeatNoticeStore(
            new Dictionary<string, LegacyLicenseSeatNotice>(),
            new List<string>());
    }

    public static ILegacyLicenseSeatNoticeStore NewCachedLegacyLicenseSeatNoticeStore(MemoryLegacyLicenseSeatNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLicenseSeatNoticeStore(
            inner,
            LegacyLicenseSeatNoticeCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLicenseSeatNotice> FindLegacyLicenseSeatNotices(ILegacyLicenseSeatNoticeStore store, LegacyLicenseSeatNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
