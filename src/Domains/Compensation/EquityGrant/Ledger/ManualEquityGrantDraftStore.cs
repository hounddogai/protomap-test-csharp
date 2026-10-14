namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual equity grant drafts by code.</summary>
public interface IManualEquityGrantDraftStore
{
    ManualEquityGrantDraft? Find(string code);
    void Save(ManualEquityGrantDraft item);
    bool Remove(string code);
    List<ManualEquityGrantDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualEquityGrantDraftStore(Dictionary<string, ManualEquityGrantDraft> items, List<string> order) : IManualEquityGrantDraftStore
{
    public Dictionary<string, ManualEquityGrantDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualEquityGrantDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualEquityGrantDraft item)
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

    public List<ManualEquityGrantDraft> List()
    {
        var outValue = new List<ManualEquityGrantDraft>();
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
public sealed class CachedManualEquityGrantDraftStore(MemoryManualEquityGrantDraftStore inner, ManualEquityGrantDraftCache cache, int hits, int misses) : IManualEquityGrantDraftStore
{
    public MemoryManualEquityGrantDraftStore Inner { get; set; } = inner;
    public ManualEquityGrantDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualEquityGrantDraft? Find(string code)
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

    public void Save(ManualEquityGrantDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualEquityGrantDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual equity grant drafts. Empty fields match all.</summary>
public sealed record ManualEquityGrantDraftQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ManualEquityGrantDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualEquityGrantDraft> Apply(List<ManualEquityGrantDraft> items)
    {
        var matched = Collections.Filter(items, (ManualEquityGrantDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual equity grant draft store.</summary>
public static class ManualEquityGrantDraftStore
{
    public static IManualEquityGrantDraftStore NewMemoryManualEquityGrantDraftStore()
    {
        return new MemoryManualEquityGrantDraftStore(
            new Dictionary<string, ManualEquityGrantDraft>(),
            new List<string>());
    }

    public static IManualEquityGrantDraftStore NewCachedManualEquityGrantDraftStore(MemoryManualEquityGrantDraftStore inner, int ttl, IClock clock)
    {
        return new CachedManualEquityGrantDraftStore(
            inner,
            ManualEquityGrantDraftCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualEquityGrantDraft> FindManualEquityGrantDrafts(IManualEquityGrantDraftStore store, ManualEquityGrantDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
