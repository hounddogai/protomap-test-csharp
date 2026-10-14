namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment notices by code.</summary>
public interface IGarnishmentNoticeStore
{
    GarnishmentNotice? Find(string code);
    void Save(GarnishmentNotice item);
    bool Remove(string code);
    List<GarnishmentNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentNoticeStore(Dictionary<string, GarnishmentNotice> items, List<string> order) : IGarnishmentNoticeStore
{
    public Dictionary<string, GarnishmentNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentNotice item)
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

    public List<GarnishmentNotice> List()
    {
        var outValue = new List<GarnishmentNotice>();
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
public sealed class CachedGarnishmentNoticeStore(MemoryGarnishmentNoticeStore inner, GarnishmentNoticeCache cache, int hits, int misses) : IGarnishmentNoticeStore
{
    public MemoryGarnishmentNoticeStore Inner { get; set; } = inner;
    public GarnishmentNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentNotice? Find(string code)
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

    public void Save(GarnishmentNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment notices. Empty fields match all.</summary>
public sealed record GarnishmentNoticeQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentNotice item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentNotice> Apply(List<GarnishmentNotice> items)
    {
        var matched = Collections.Filter(items, (GarnishmentNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment notice store.</summary>
public static class GarnishmentNoticeStore
{
    public static IGarnishmentNoticeStore NewMemoryGarnishmentNoticeStore()
    {
        return new MemoryGarnishmentNoticeStore(new Dictionary<string, GarnishmentNotice>(), new List<string>());
    }

    public static IGarnishmentNoticeStore NewCachedGarnishmentNoticeStore(MemoryGarnishmentNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentNoticeStore(inner, GarnishmentNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentNotice> FindGarnishmentNotices(IGarnishmentNoticeStore store, GarnishmentNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
