namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor drafts by code.</summary>
public interface IVisitorDraftStore
{
    VisitorDraft? Find(string code);
    void Save(VisitorDraft item);
    bool Remove(string code);
    List<VisitorDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorDraftStore(Dictionary<string, VisitorDraft> items, List<string> order) : IVisitorDraftStore
{
    public Dictionary<string, VisitorDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorDraft item)
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

    public List<VisitorDraft> List()
    {
        var outValue = new List<VisitorDraft>();
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
public sealed class CachedVisitorDraftStore(MemoryVisitorDraftStore inner, VisitorDraftCache cache, int hits, int misses) : IVisitorDraftStore
{
    public MemoryVisitorDraftStore Inner { get; set; } = inner;
    public VisitorDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorDraft? Find(string code)
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

    public void Save(VisitorDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor drafts. Empty fields match all.</summary>
public sealed record VisitorDraftQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<VisitorDraft> Apply(List<VisitorDraft> items)
    {
        var matched = Collections.Filter(items, (VisitorDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor draft store.</summary>
public static class VisitorDraftStore
{
    public static IVisitorDraftStore NewMemoryVisitorDraftStore()
    {
        return new MemoryVisitorDraftStore(new Dictionary<string, VisitorDraft>(), new List<string>());
    }

    public static IVisitorDraftStore NewCachedVisitorDraftStore(MemoryVisitorDraftStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorDraftStore(inner, VisitorDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorDraft> FindVisitorDrafts(IVisitorDraftStore store, VisitorDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
