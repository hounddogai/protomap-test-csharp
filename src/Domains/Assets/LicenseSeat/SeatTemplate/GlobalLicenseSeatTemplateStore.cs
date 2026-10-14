namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global license seat templates by code.</summary>
public interface IGlobalLicenseSeatTemplateStore
{
    GlobalLicenseSeatTemplate? Find(string code);
    void Save(GlobalLicenseSeatTemplate item);
    bool Remove(string code);
    List<GlobalLicenseSeatTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalLicenseSeatTemplateStore(Dictionary<string, GlobalLicenseSeatTemplate> items, List<string> order) : IGlobalLicenseSeatTemplateStore
{
    public Dictionary<string, GlobalLicenseSeatTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalLicenseSeatTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalLicenseSeatTemplate item)
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

    public List<GlobalLicenseSeatTemplate> List()
    {
        var outValue = new List<GlobalLicenseSeatTemplate>();
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
public sealed class CachedGlobalLicenseSeatTemplateStore(MemoryGlobalLicenseSeatTemplateStore inner, GlobalLicenseSeatTemplateCache cache, int hits, int misses) : IGlobalLicenseSeatTemplateStore
{
    public MemoryGlobalLicenseSeatTemplateStore Inner { get; set; } = inner;
    public GlobalLicenseSeatTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalLicenseSeatTemplate? Find(string code)
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

    public void Save(GlobalLicenseSeatTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalLicenseSeatTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global license seat templates. Empty fields match all.</summary>
public sealed record GlobalLicenseSeatTemplateQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalLicenseSeatTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalLicenseSeatTemplate> Apply(List<GlobalLicenseSeatTemplate> items)
    {
        var matched = Collections.Filter(items, (GlobalLicenseSeatTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global license seat template store.</summary>
public static class GlobalLicenseSeatTemplateStore
{
    public static IGlobalLicenseSeatTemplateStore NewMemoryGlobalLicenseSeatTemplateStore()
    {
        return new MemoryGlobalLicenseSeatTemplateStore(
            new Dictionary<string, GlobalLicenseSeatTemplate>(),
            new List<string>());
    }

    public static IGlobalLicenseSeatTemplateStore NewCachedGlobalLicenseSeatTemplateStore(MemoryGlobalLicenseSeatTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalLicenseSeatTemplateStore(
            inner,
            GlobalLicenseSeatTemplateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalLicenseSeatTemplate> FindGlobalLicenseSeatTemplates(IGlobalLicenseSeatTemplateStore store, GlobalLicenseSeatTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
