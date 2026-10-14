namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop drafts by code.</summary>
public interface ILaptopDraftStore
{
    LaptopDraft? Find(string code);
    void Save(LaptopDraft item);
    bool Remove(string code);
    List<LaptopDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopDraftStore(Dictionary<string, LaptopDraft> items, List<string> order) : ILaptopDraftStore
{
    public Dictionary<string, LaptopDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopDraft item)
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

    public List<LaptopDraft> List()
    {
        var outValue = new List<LaptopDraft>();
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
public sealed class CachedLaptopDraftStore(MemoryLaptopDraftStore inner, LaptopDraftCache cache, int hits, int misses) : ILaptopDraftStore
{
    public MemoryLaptopDraftStore Inner { get; set; } = inner;
    public LaptopDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopDraft? Find(string code)
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

    public void Save(LaptopDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop drafts. Empty fields match all.</summary>
public sealed record LaptopDraftQuery(string Department, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopDraft item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LaptopDraft> Apply(List<LaptopDraft> items)
    {
        var matched = Collections.Filter(items, (LaptopDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop draft store.</summary>
public static class LaptopDraftStore
{
    public static ILaptopDraftStore NewMemoryLaptopDraftStore()
    {
        return new MemoryLaptopDraftStore(new Dictionary<string, LaptopDraft>(), new List<string>());
    }

    public static ILaptopDraftStore NewCachedLaptopDraftStore(MemoryLaptopDraftStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopDraftStore(inner, LaptopDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LaptopDraft> FindLaptopDrafts(ILaptopDraftStore store, LaptopDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
