namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination notices by code.</summary>
public interface INominationNoticeStore
{
    NominationNotice? Find(string code);
    void Save(NominationNotice item);
    bool Remove(string code);
    List<NominationNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationNoticeStore(Dictionary<string, NominationNotice> items, List<string> order) : INominationNoticeStore
{
    public Dictionary<string, NominationNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationNotice item)
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

    public List<NominationNotice> List()
    {
        var outValue = new List<NominationNotice>();
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
public sealed class CachedNominationNoticeStore(MemoryNominationNoticeStore inner, NominationNoticeCache cache, int hits, int misses) : INominationNoticeStore
{
    public MemoryNominationNoticeStore Inner { get; set; } = inner;
    public NominationNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationNotice? Find(string code)
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

    public void Save(NominationNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination notices. Empty fields match all.</summary>
public sealed record NominationNoticeQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(NominationNotice item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationNotice> Apply(List<NominationNotice> items)
    {
        var matched = Collections.Filter(items, (NominationNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination notice store.</summary>
public static class NominationNoticeStore
{
    public static INominationNoticeStore NewMemoryNominationNoticeStore()
    {
        return new MemoryNominationNoticeStore(new Dictionary<string, NominationNotice>(), new List<string>());
    }

    public static INominationNoticeStore NewCachedNominationNoticeStore(MemoryNominationNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedNominationNoticeStore(inner, NominationNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationNotice> FindNominationNotices(INominationNoticeStore store, NominationNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
