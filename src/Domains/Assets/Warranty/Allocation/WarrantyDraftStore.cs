namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty drafts by code.</summary>
public interface IWarrantyDraftStore
{
    WarrantyDraft? Find(string code);
    void Save(WarrantyDraft item);
    bool Remove(string code);
    List<WarrantyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyDraftStore(Dictionary<string, WarrantyDraft> items, List<string> order) : IWarrantyDraftStore
{
    public Dictionary<string, WarrantyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyDraft item)
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

    public List<WarrantyDraft> List()
    {
        var outValue = new List<WarrantyDraft>();
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
public sealed class CachedWarrantyDraftStore(MemoryWarrantyDraftStore inner, WarrantyDraftCache cache, int hits, int misses) : IWarrantyDraftStore
{
    public MemoryWarrantyDraftStore Inner { get; set; } = inner;
    public WarrantyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyDraft? Find(string code)
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

    public void Save(WarrantyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty drafts. Empty fields match all.</summary>
public sealed record WarrantyDraftQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyDraft> Apply(List<WarrantyDraft> items)
    {
        var matched = Collections.Filter(items, (WarrantyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty draft store.</summary>
public static class WarrantyDraftStore
{
    public static IWarrantyDraftStore NewMemoryWarrantyDraftStore()
    {
        return new MemoryWarrantyDraftStore(new Dictionary<string, WarrantyDraft>(), new List<string>());
    }

    public static IWarrantyDraftStore NewCachedWarrantyDraftStore(MemoryWarrantyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyDraftStore(inner, WarrantyDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyDraft> FindWarrantyDrafts(IWarrantyDraftStore store, WarrantyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
