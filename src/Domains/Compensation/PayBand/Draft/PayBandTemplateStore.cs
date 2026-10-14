namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band templates by code.</summary>
public interface IPayBandTemplateStore
{
    PayBandTemplate? Find(string code);
    void Save(PayBandTemplate item);
    bool Remove(string code);
    List<PayBandTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandTemplateStore(Dictionary<string, PayBandTemplate> items, List<string> order) : IPayBandTemplateStore
{
    public Dictionary<string, PayBandTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandTemplate item)
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

    public List<PayBandTemplate> List()
    {
        var outValue = new List<PayBandTemplate>();
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
public sealed class CachedPayBandTemplateStore(MemoryPayBandTemplateStore inner, PayBandTemplateCache cache, int hits, int misses) : IPayBandTemplateStore
{
    public MemoryPayBandTemplateStore Inner { get; set; } = inner;
    public PayBandTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandTemplate? Find(string code)
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

    public void Save(PayBandTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band templates. Empty fields match all.</summary>
public sealed record PayBandTemplateQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandTemplate> Apply(List<PayBandTemplate> items)
    {
        var matched = Collections.Filter(items, (PayBandTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band template store.</summary>
public static class PayBandTemplateStore
{
    public static IPayBandTemplateStore NewMemoryPayBandTemplateStore()
    {
        return new MemoryPayBandTemplateStore(new Dictionary<string, PayBandTemplate>(), new List<string>());
    }

    public static IPayBandTemplateStore NewCachedPayBandTemplateStore(MemoryPayBandTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandTemplateStore(inner, PayBandTemplateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandTemplate> FindPayBandTemplates(IPayBandTemplateStore store, PayBandTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
