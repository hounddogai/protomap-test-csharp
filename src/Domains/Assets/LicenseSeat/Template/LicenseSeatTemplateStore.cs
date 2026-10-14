namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat templates by code.</summary>
public interface ILicenseSeatTemplateStore
{
    LicenseSeatTemplate? Find(string code);
    void Save(LicenseSeatTemplate item);
    bool Remove(string code);
    List<LicenseSeatTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatTemplateStore(Dictionary<string, LicenseSeatTemplate> items, List<string> order) : ILicenseSeatTemplateStore
{
    public Dictionary<string, LicenseSeatTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatTemplate item)
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

    public List<LicenseSeatTemplate> List()
    {
        var outValue = new List<LicenseSeatTemplate>();
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
public sealed class CachedLicenseSeatTemplateStore(MemoryLicenseSeatTemplateStore inner, LicenseSeatTemplateCache cache, int hits, int misses) : ILicenseSeatTemplateStore
{
    public MemoryLicenseSeatTemplateStore Inner { get; set; } = inner;
    public LicenseSeatTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatTemplate? Find(string code)
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

    public void Save(LicenseSeatTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat templates. Empty fields match all.</summary>
public sealed record LicenseSeatTemplateQuery(string Locale, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatTemplate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatTemplate> Apply(List<LicenseSeatTemplate> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat template store.</summary>
public static class LicenseSeatTemplateStore
{
    public static ILicenseSeatTemplateStore NewMemoryLicenseSeatTemplateStore()
    {
        return new MemoryLicenseSeatTemplateStore(new Dictionary<string, LicenseSeatTemplate>(), new List<string>());
    }

    public static ILicenseSeatTemplateStore NewCachedLicenseSeatTemplateStore(MemoryLicenseSeatTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatTemplateStore(inner, LicenseSeatTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatTemplate> FindLicenseSeatTemplates(ILicenseSeatTemplateStore store, LicenseSeatTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
