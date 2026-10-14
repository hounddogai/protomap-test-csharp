namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle notices by code.</summary>
public interface IMeritCycleNoticeStore
{
    MeritCycleNotice? Find(string code);
    void Save(MeritCycleNotice item);
    bool Remove(string code);
    List<MeritCycleNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleNoticeStore(Dictionary<string, MeritCycleNotice> items, List<string> order) : IMeritCycleNoticeStore
{
    public Dictionary<string, MeritCycleNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleNotice item)
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

    public List<MeritCycleNotice> List()
    {
        var outValue = new List<MeritCycleNotice>();
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
public sealed class CachedMeritCycleNoticeStore(MemoryMeritCycleNoticeStore inner, MeritCycleNoticeCache cache, int hits, int misses) : IMeritCycleNoticeStore
{
    public MemoryMeritCycleNoticeStore Inner { get; set; } = inner;
    public MeritCycleNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleNotice? Find(string code)
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

    public void Save(MeritCycleNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle notices. Empty fields match all.</summary>
public sealed record MeritCycleNoticeQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleNotice> Apply(List<MeritCycleNotice> items)
    {
        var matched = Collections.Filter(items, (MeritCycleNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle notice store.</summary>
public static class MeritCycleNoticeStore
{
    public static IMeritCycleNoticeStore NewMemoryMeritCycleNoticeStore()
    {
        return new MemoryMeritCycleNoticeStore(new Dictionary<string, MeritCycleNotice>(), new List<string>());
    }

    public static IMeritCycleNoticeStore NewCachedMeritCycleNoticeStore(MemoryMeritCycleNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleNoticeStore(inner, MeritCycleNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MeritCycleNotice> FindMeritCycleNotices(IMeritCycleNoticeStore store, MeritCycleNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
