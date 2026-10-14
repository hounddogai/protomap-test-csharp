namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending balance templates by code.</summary>
public interface IPendingBalanceTemplateStore
{
    PendingBalanceTemplate? Find(string code);
    void Save(PendingBalanceTemplate item);
    bool Remove(string code);
    List<PendingBalanceTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBalanceTemplateStore(Dictionary<string, PendingBalanceTemplate> items, List<string> order) : IPendingBalanceTemplateStore
{
    public Dictionary<string, PendingBalanceTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBalanceTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBalanceTemplate item)
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

    public List<PendingBalanceTemplate> List()
    {
        var outValue = new List<PendingBalanceTemplate>();
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
public sealed class CachedPendingBalanceTemplateStore(MemoryPendingBalanceTemplateStore inner, PendingBalanceTemplateCache cache, int hits, int misses) : IPendingBalanceTemplateStore
{
    public MemoryPendingBalanceTemplateStore Inner { get; set; } = inner;
    public PendingBalanceTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBalanceTemplate? Find(string code)
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

    public void Save(PendingBalanceTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBalanceTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending balance templates. Empty fields match all.</summary>
public sealed record PendingBalanceTemplateQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBalanceTemplate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingBalanceTemplate> Apply(List<PendingBalanceTemplate> items)
    {
        var matched = Collections.Filter(items, (PendingBalanceTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending balance template store.</summary>
public static class PendingBalanceTemplateStore
{
    public static IPendingBalanceTemplateStore NewMemoryPendingBalanceTemplateStore()
    {
        return new MemoryPendingBalanceTemplateStore(
            new Dictionary<string, PendingBalanceTemplate>(),
            new List<string>());
    }

    public static IPendingBalanceTemplateStore NewCachedPendingBalanceTemplateStore(MemoryPendingBalanceTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBalanceTemplateStore(
            inner,
            PendingBalanceTemplateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingBalanceTemplate> FindPendingBalanceTemplates(IPendingBalanceTemplateStore store, PendingBalanceTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
