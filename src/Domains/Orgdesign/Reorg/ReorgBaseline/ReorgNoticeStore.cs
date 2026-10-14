namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg notices by code.</summary>
public interface IReorgNoticeStore
{
    ReorgNotice? Find(string code);
    void Save(ReorgNotice item);
    bool Remove(string code);
    List<ReorgNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgNoticeStore(Dictionary<string, ReorgNotice> items, List<string> order) : IReorgNoticeStore
{
    public Dictionary<string, ReorgNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgNotice item)
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

    public List<ReorgNotice> List()
    {
        var outValue = new List<ReorgNotice>();
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
public sealed class CachedReorgNoticeStore(MemoryReorgNoticeStore inner, ReorgNoticeCache cache, int hits, int misses) : IReorgNoticeStore
{
    public MemoryReorgNoticeStore Inner { get; set; } = inner;
    public ReorgNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgNotice? Find(string code)
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

    public void Save(ReorgNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg notices. Empty fields match all.</summary>
public sealed record ReorgNoticeQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgNotice item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ReorgNotice> Apply(List<ReorgNotice> items)
    {
        var matched = Collections.Filter(items, (ReorgNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg notice store.</summary>
public static class ReorgNoticeStore
{
    public static IReorgNoticeStore NewMemoryReorgNoticeStore()
    {
        return new MemoryReorgNoticeStore(new Dictionary<string, ReorgNotice>(), new List<string>());
    }

    public static IReorgNoticeStore NewCachedReorgNoticeStore(MemoryReorgNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedReorgNoticeStore(inner, ReorgNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgNotice> FindReorgNotices(IReorgNoticeStore store, ReorgNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
