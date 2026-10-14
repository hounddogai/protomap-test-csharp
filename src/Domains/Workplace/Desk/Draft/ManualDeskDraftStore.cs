namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual desk drafts by code.</summary>
public interface IManualDeskDraftStore
{
    ManualDeskDraft? Find(string code);
    void Save(ManualDeskDraft item);
    bool Remove(string code);
    List<ManualDeskDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualDeskDraftStore(Dictionary<string, ManualDeskDraft> items, List<string> order) : IManualDeskDraftStore
{
    public Dictionary<string, ManualDeskDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualDeskDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualDeskDraft item)
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

    public List<ManualDeskDraft> List()
    {
        var outValue = new List<ManualDeskDraft>();
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
public sealed class CachedManualDeskDraftStore(MemoryManualDeskDraftStore inner, ManualDeskDraftCache cache, int hits, int misses) : IManualDeskDraftStore
{
    public MemoryManualDeskDraftStore Inner { get; set; } = inner;
    public ManualDeskDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualDeskDraft? Find(string code)
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

    public void Save(ManualDeskDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualDeskDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual desk drafts. Empty fields match all.</summary>
public sealed record ManualDeskDraftQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ManualDeskDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualDeskDraft> Apply(List<ManualDeskDraft> items)
    {
        var matched = Collections.Filter(items, (ManualDeskDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual desk draft store.</summary>
public static class ManualDeskDraftStore
{
    public static IManualDeskDraftStore NewMemoryManualDeskDraftStore()
    {
        return new MemoryManualDeskDraftStore(new Dictionary<string, ManualDeskDraft>(), new List<string>());
    }

    public static IManualDeskDraftStore NewCachedManualDeskDraftStore(MemoryManualDeskDraftStore inner, int ttl, IClock clock)
    {
        return new CachedManualDeskDraftStore(inner, ManualDeskDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualDeskDraft> FindManualDeskDrafts(IManualDeskDraftStore store, ManualDeskDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
