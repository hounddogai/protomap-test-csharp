namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk notices by code.</summary>
public interface IDeskNoticeStore
{
    DeskNotice? Find(string code);
    void Save(DeskNotice item);
    bool Remove(string code);
    List<DeskNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskNoticeStore(Dictionary<string, DeskNotice> items, List<string> order) : IDeskNoticeStore
{
    public Dictionary<string, DeskNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskNotice item)
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

    public List<DeskNotice> List()
    {
        var outValue = new List<DeskNotice>();
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
public sealed class CachedDeskNoticeStore(MemoryDeskNoticeStore inner, DeskNoticeCache cache, int hits, int misses) : IDeskNoticeStore
{
    public MemoryDeskNoticeStore Inner { get; set; } = inner;
    public DeskNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskNotice? Find(string code)
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

    public void Save(DeskNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk notices. Empty fields match all.</summary>
public sealed record DeskNoticeQuery(string Team, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DeskNotice item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskNotice> Apply(List<DeskNotice> items)
    {
        var matched = Collections.Filter(items, (DeskNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk notice store.</summary>
public static class DeskNoticeStore
{
    public static IDeskNoticeStore NewMemoryDeskNoticeStore()
    {
        return new MemoryDeskNoticeStore(new Dictionary<string, DeskNotice>(), new List<string>());
    }

    public static IDeskNoticeStore NewCachedDeskNoticeStore(MemoryDeskNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedDeskNoticeStore(inner, DeskNoticeCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskNotice> FindDeskNotices(IDeskNoticeStore store, DeskNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
