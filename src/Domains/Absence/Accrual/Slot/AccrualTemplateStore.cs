namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual templates by code.</summary>
public interface IAccrualTemplateStore
{
    AccrualTemplate? Find(string code);
    void Save(AccrualTemplate item);
    bool Remove(string code);
    List<AccrualTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualTemplateStore(Dictionary<string, AccrualTemplate> items, List<string> order) : IAccrualTemplateStore
{
    public Dictionary<string, AccrualTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualTemplate item)
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

    public List<AccrualTemplate> List()
    {
        var outValue = new List<AccrualTemplate>();
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
public sealed class CachedAccrualTemplateStore(MemoryAccrualTemplateStore inner, AccrualTemplateCache cache, int hits, int misses) : IAccrualTemplateStore
{
    public MemoryAccrualTemplateStore Inner { get; set; } = inner;
    public AccrualTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualTemplate? Find(string code)
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

    public void Save(AccrualTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual templates. Empty fields match all.</summary>
public sealed record AccrualTemplateQuery(string Segment, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualTemplate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualTemplate> Apply(List<AccrualTemplate> items)
    {
        var matched = Collections.Filter(items, (AccrualTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual template store.</summary>
public static class AccrualTemplateStore
{
    public static IAccrualTemplateStore NewMemoryAccrualTemplateStore()
    {
        return new MemoryAccrualTemplateStore(new Dictionary<string, AccrualTemplate>(), new List<string>());
    }

    public static IAccrualTemplateStore NewCachedAccrualTemplateStore(MemoryAccrualTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualTemplateStore(inner, AccrualTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccrualTemplate> FindAccrualTemplates(IAccrualTemplateStore store, AccrualTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
