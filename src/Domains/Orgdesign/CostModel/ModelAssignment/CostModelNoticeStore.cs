namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model notices by code.</summary>
public interface ICostModelNoticeStore
{
    CostModelNotice? Find(string code);
    void Save(CostModelNotice item);
    bool Remove(string code);
    List<CostModelNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelNoticeStore(Dictionary<string, CostModelNotice> items, List<string> order) : ICostModelNoticeStore
{
    public Dictionary<string, CostModelNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelNotice item)
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

    public List<CostModelNotice> List()
    {
        var outValue = new List<CostModelNotice>();
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
public sealed class CachedCostModelNoticeStore(MemoryCostModelNoticeStore inner, CostModelNoticeCache cache, int hits, int misses) : ICostModelNoticeStore
{
    public MemoryCostModelNoticeStore Inner { get; set; } = inner;
    public CostModelNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelNotice? Find(string code)
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

    public void Save(CostModelNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model notices. Empty fields match all.</summary>
public sealed record CostModelNoticeQuery(string Region, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelNotice item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelNotice> Apply(List<CostModelNotice> items)
    {
        var matched = Collections.Filter(items, (CostModelNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model notice store.</summary>
public static class CostModelNoticeStore
{
    public static ICostModelNoticeStore NewMemoryCostModelNoticeStore()
    {
        return new MemoryCostModelNoticeStore(new Dictionary<string, CostModelNotice>(), new List<string>());
    }

    public static ICostModelNoticeStore NewCachedCostModelNoticeStore(MemoryCostModelNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelNoticeStore(inner, CostModelNoticeCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelNotice> FindCostModelNotices(ICostModelNoticeStore store, CostModelNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
