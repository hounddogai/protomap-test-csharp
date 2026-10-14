namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual bonus pool drafts by code.</summary>
public interface IManualBonusPoolDraftStore
{
    ManualBonusPoolDraft? Find(string code);
    void Save(ManualBonusPoolDraft item);
    bool Remove(string code);
    List<ManualBonusPoolDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBonusPoolDraftStore(Dictionary<string, ManualBonusPoolDraft> items, List<string> order) : IManualBonusPoolDraftStore
{
    public Dictionary<string, ManualBonusPoolDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBonusPoolDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBonusPoolDraft item)
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

    public List<ManualBonusPoolDraft> List()
    {
        var outValue = new List<ManualBonusPoolDraft>();
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
public sealed class CachedManualBonusPoolDraftStore(MemoryManualBonusPoolDraftStore inner, ManualBonusPoolDraftCache cache, int hits, int misses) : IManualBonusPoolDraftStore
{
    public MemoryManualBonusPoolDraftStore Inner { get; set; } = inner;
    public ManualBonusPoolDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBonusPoolDraft? Find(string code)
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

    public void Save(ManualBonusPoolDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBonusPoolDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual bonus pool drafts. Empty fields match all.</summary>
public sealed record ManualBonusPoolDraftQuery(string Channel, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBonusPoolDraft item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBonusPoolDraft> Apply(List<ManualBonusPoolDraft> items)
    {
        var matched = Collections.Filter(items, (ManualBonusPoolDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual bonus pool draft store.</summary>
public static class ManualBonusPoolDraftStore
{
    public static IManualBonusPoolDraftStore NewMemoryManualBonusPoolDraftStore()
    {
        return new MemoryManualBonusPoolDraftStore(
            new Dictionary<string, ManualBonusPoolDraft>(),
            new List<string>());
    }

    public static IManualBonusPoolDraftStore NewCachedManualBonusPoolDraftStore(MemoryManualBonusPoolDraftStore inner, int ttl, IClock clock)
    {
        return new CachedManualBonusPoolDraftStore(inner, ManualBonusPoolDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualBonusPoolDraft> FindManualBonusPoolDrafts(IManualBonusPoolDraftStore store, ManualBonusPoolDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
