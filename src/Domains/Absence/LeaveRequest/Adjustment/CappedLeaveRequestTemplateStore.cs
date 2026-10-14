namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped leave request templates by code.</summary>
public interface ICappedLeaveRequestTemplateStore
{
    CappedLeaveRequestTemplate? Find(string code);
    void Save(CappedLeaveRequestTemplate item);
    bool Remove(string code);
    List<CappedLeaveRequestTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLeaveRequestTemplateStore(Dictionary<string, CappedLeaveRequestTemplate> items, List<string> order) : ICappedLeaveRequestTemplateStore
{
    public Dictionary<string, CappedLeaveRequestTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLeaveRequestTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLeaveRequestTemplate item)
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

    public List<CappedLeaveRequestTemplate> List()
    {
        var outValue = new List<CappedLeaveRequestTemplate>();
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
public sealed class CachedCappedLeaveRequestTemplateStore(MemoryCappedLeaveRequestTemplateStore inner, CappedLeaveRequestTemplateCache cache, int hits, int misses) : ICappedLeaveRequestTemplateStore
{
    public MemoryCappedLeaveRequestTemplateStore Inner { get; set; } = inner;
    public CappedLeaveRequestTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLeaveRequestTemplate? Find(string code)
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

    public void Save(CappedLeaveRequestTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLeaveRequestTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped leave request templates. Empty fields match all.</summary>
public sealed record CappedLeaveRequestTemplateQuery(string Department, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLeaveRequestTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedLeaveRequestTemplate> Apply(List<CappedLeaveRequestTemplate> items)
    {
        var matched = Collections.Filter(items, (CappedLeaveRequestTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped leave request template store.</summary>
public static class CappedLeaveRequestTemplateStore
{
    public static ICappedLeaveRequestTemplateStore NewMemoryCappedLeaveRequestTemplateStore()
    {
        return new MemoryCappedLeaveRequestTemplateStore(
            new Dictionary<string, CappedLeaveRequestTemplate>(),
            new List<string>());
    }

    public static ICappedLeaveRequestTemplateStore NewCachedCappedLeaveRequestTemplateStore(MemoryCappedLeaveRequestTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLeaveRequestTemplateStore(
            inner,
            CappedLeaveRequestTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedLeaveRequestTemplate> FindCappedLeaveRequestTemplates(ICappedLeaveRequestTemplateStore store, CappedLeaveRequestTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
