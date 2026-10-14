namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary bonus pool notices by code.</summary>
public interface IPrimaryBonusPoolNoticeStore
{
    PrimaryBonusPoolNotice? Find(string code);
    void Save(PrimaryBonusPoolNotice item);
    bool Remove(string code);
    List<PrimaryBonusPoolNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryBonusPoolNoticeStore(Dictionary<string, PrimaryBonusPoolNotice> items, List<string> order) : IPrimaryBonusPoolNoticeStore
{
    public Dictionary<string, PrimaryBonusPoolNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryBonusPoolNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryBonusPoolNotice item)
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

    public List<PrimaryBonusPoolNotice> List()
    {
        var outValue = new List<PrimaryBonusPoolNotice>();
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
public sealed class CachedPrimaryBonusPoolNoticeStore(MemoryPrimaryBonusPoolNoticeStore inner, PrimaryBonusPoolNoticeCache cache, int hits, int misses) : IPrimaryBonusPoolNoticeStore
{
    public MemoryPrimaryBonusPoolNoticeStore Inner { get; set; } = inner;
    public PrimaryBonusPoolNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryBonusPoolNotice? Find(string code)
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

    public void Save(PrimaryBonusPoolNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryBonusPoolNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary bonus pool notices. Empty fields match all.</summary>
public sealed record PrimaryBonusPoolNoticeQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryBonusPoolNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryBonusPoolNotice> Apply(List<PrimaryBonusPoolNotice> items)
    {
        var matched = Collections.Filter(items, (PrimaryBonusPoolNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary bonus pool notice store.</summary>
public static class PrimaryBonusPoolNoticeStore
{
    public static IPrimaryBonusPoolNoticeStore NewMemoryPrimaryBonusPoolNoticeStore()
    {
        return new MemoryPrimaryBonusPoolNoticeStore(
            new Dictionary<string, PrimaryBonusPoolNotice>(),
            new List<string>());
    }

    public static IPrimaryBonusPoolNoticeStore NewCachedPrimaryBonusPoolNoticeStore(MemoryPrimaryBonusPoolNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryBonusPoolNoticeStore(
            inner,
            PrimaryBonusPoolNoticeCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryBonusPoolNotice> FindPrimaryBonusPoolNotices(IPrimaryBonusPoolNoticeStore store, PrimaryBonusPoolNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
