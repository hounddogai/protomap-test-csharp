namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run templates by code.</summary>
public interface IPayRunTemplateStore
{
    PayRunTemplate? Find(string code);
    void Save(PayRunTemplate item);
    bool Remove(string code);
    List<PayRunTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunTemplateStore(Dictionary<string, PayRunTemplate> items, List<string> order) : IPayRunTemplateStore
{
    public Dictionary<string, PayRunTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunTemplate item)
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

    public List<PayRunTemplate> List()
    {
        var outValue = new List<PayRunTemplate>();
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
public sealed class CachedPayRunTemplateStore(MemoryPayRunTemplateStore inner, PayRunTemplateCache cache, int hits, int misses) : IPayRunTemplateStore
{
    public MemoryPayRunTemplateStore Inner { get; set; } = inner;
    public PayRunTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunTemplate? Find(string code)
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

    public void Save(PayRunTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run templates. Empty fields match all.</summary>
public sealed record PayRunTemplateQuery(string Label, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PayRunTemplate> Apply(List<PayRunTemplate> items)
    {
        var matched = Collections.Filter(items, (PayRunTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run template store.</summary>
public static class PayRunTemplateStore
{
    public static IPayRunTemplateStore NewMemoryPayRunTemplateStore()
    {
        return new MemoryPayRunTemplateStore(new Dictionary<string, PayRunTemplate>(), new List<string>());
    }

    public static IPayRunTemplateStore NewCachedPayRunTemplateStore(MemoryPayRunTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunTemplateStore(inner, PayRunTemplateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunTemplate> FindPayRunTemplates(IPayRunTemplateStore store, PayRunTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
