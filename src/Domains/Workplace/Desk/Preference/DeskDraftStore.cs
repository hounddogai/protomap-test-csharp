namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk drafts by code.</summary>
public interface IDeskDraftStore
{
    DeskDraft? Find(string code);
    void Save(DeskDraft item);
    bool Remove(string code);
    List<DeskDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskDraftStore(Dictionary<string, DeskDraft> items, List<string> order) : IDeskDraftStore
{
    public Dictionary<string, DeskDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskDraft item)
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

    public List<DeskDraft> List()
    {
        var outValue = new List<DeskDraft>();
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
public sealed class CachedDeskDraftStore(MemoryDeskDraftStore inner, DeskDraftCache cache, int hits, int misses) : IDeskDraftStore
{
    public MemoryDeskDraftStore Inner { get; set; } = inner;
    public DeskDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskDraft? Find(string code)
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

    public void Save(DeskDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk drafts. Empty fields match all.</summary>
public sealed record DeskDraftQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(DeskDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskDraft> Apply(List<DeskDraft> items)
    {
        var matched = Collections.Filter(items, (DeskDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk draft store.</summary>
public static class DeskDraftStore
{
    public static IDeskDraftStore NewMemoryDeskDraftStore()
    {
        return new MemoryDeskDraftStore(new Dictionary<string, DeskDraft>(), new List<string>());
    }

    public static IDeskDraftStore NewCachedDeskDraftStore(MemoryDeskDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDeskDraftStore(inner, DeskDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeskDraft> FindDeskDrafts(IDeskDraftStore store, DeskDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
