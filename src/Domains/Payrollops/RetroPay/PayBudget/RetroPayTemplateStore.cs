namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay templates by code.</summary>
public interface IRetroPayTemplateStore
{
    RetroPayTemplate? Find(string code);
    void Save(RetroPayTemplate item);
    bool Remove(string code);
    List<RetroPayTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayTemplateStore(Dictionary<string, RetroPayTemplate> items, List<string> order) : IRetroPayTemplateStore
{
    public Dictionary<string, RetroPayTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayTemplate item)
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

    public List<RetroPayTemplate> List()
    {
        var outValue = new List<RetroPayTemplate>();
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
public sealed class CachedRetroPayTemplateStore(MemoryRetroPayTemplateStore inner, RetroPayTemplateCache cache, int hits, int misses) : IRetroPayTemplateStore
{
    public MemoryRetroPayTemplateStore Inner { get; set; } = inner;
    public RetroPayTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayTemplate? Find(string code)
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

    public void Save(RetroPayTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay templates. Empty fields match all.</summary>
public sealed record RetroPayTemplateQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayTemplate> Apply(List<RetroPayTemplate> items)
    {
        var matched = Collections.Filter(items, (RetroPayTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay template store.</summary>
public static class RetroPayTemplateStore
{
    public static IRetroPayTemplateStore NewMemoryRetroPayTemplateStore()
    {
        return new MemoryRetroPayTemplateStore(new Dictionary<string, RetroPayTemplate>(), new List<string>());
    }

    public static IRetroPayTemplateStore NewCachedRetroPayTemplateStore(MemoryRetroPayTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayTemplateStore(inner, RetroPayTemplateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayTemplate> FindRetroPayTemplates(IRetroPayTemplateStore store, RetroPayTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
