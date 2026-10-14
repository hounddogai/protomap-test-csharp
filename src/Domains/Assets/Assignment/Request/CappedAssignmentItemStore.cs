namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped assignment items by code.</summary>
public interface ICappedAssignmentItemStore
{
    CappedAssignmentItem? Find(string code);
    void Save(CappedAssignmentItem item);
    bool Remove(string code);
    List<CappedAssignmentItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAssignmentItemStore(Dictionary<string, CappedAssignmentItem> items, List<string> order) : ICappedAssignmentItemStore
{
    public Dictionary<string, CappedAssignmentItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAssignmentItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAssignmentItem item)
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

    public List<CappedAssignmentItem> List()
    {
        var outValue = new List<CappedAssignmentItem>();
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
public sealed class CachedCappedAssignmentItemStore(MemoryCappedAssignmentItemStore inner, CappedAssignmentItemCache cache, int hits, int misses) : ICappedAssignmentItemStore
{
    public MemoryCappedAssignmentItemStore Inner { get; set; } = inner;
    public CappedAssignmentItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAssignmentItem? Find(string code)
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

    public void Save(CappedAssignmentItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAssignmentItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped assignment items. Empty fields match all.</summary>
public sealed record CappedAssignmentItemQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAssignmentItem item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAssignmentItem> Apply(List<CappedAssignmentItem> items)
    {
        var matched = Collections.Filter(items, (CappedAssignmentItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped assignment item store.</summary>
public static class CappedAssignmentItemStore
{
    public static ICappedAssignmentItemStore NewMemoryCappedAssignmentItemStore()
    {
        return new MemoryCappedAssignmentItemStore(
            new Dictionary<string, CappedAssignmentItem>(),
            new List<string>());
    }

    public static ICappedAssignmentItemStore NewCachedCappedAssignmentItemStore(MemoryCappedAssignmentItemStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAssignmentItemStore(inner, CappedAssignmentItemCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedAssignmentItem> FindCappedAssignmentItems(ICappedAssignmentItemStore store, CappedAssignmentItemQuery query)
    {
        return query.Apply(store.List());
    }
}
