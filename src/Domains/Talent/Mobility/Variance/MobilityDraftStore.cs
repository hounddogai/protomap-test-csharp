namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility drafts by code.</summary>
public interface IMobilityDraftStore
{
    MobilityDraft? Find(string code);
    void Save(MobilityDraft item);
    bool Remove(string code);
    List<MobilityDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityDraftStore(Dictionary<string, MobilityDraft> items, List<string> order) : IMobilityDraftStore
{
    public Dictionary<string, MobilityDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityDraft item)
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

    public List<MobilityDraft> List()
    {
        var outValue = new List<MobilityDraft>();
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
public sealed class CachedMobilityDraftStore(MemoryMobilityDraftStore inner, MobilityDraftCache cache, int hits, int misses) : IMobilityDraftStore
{
    public MemoryMobilityDraftStore Inner { get; set; } = inner;
    public MobilityDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityDraft? Find(string code)
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

    public void Save(MobilityDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility drafts. Empty fields match all.</summary>
public sealed record MobilityDraftQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityDraft item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityDraft> Apply(List<MobilityDraft> items)
    {
        var matched = Collections.Filter(items, (MobilityDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility draft store.</summary>
public static class MobilityDraftStore
{
    public static IMobilityDraftStore NewMemoryMobilityDraftStore()
    {
        return new MemoryMobilityDraftStore(new Dictionary<string, MobilityDraft>(), new List<string>());
    }

    public static IMobilityDraftStore NewCachedMobilityDraftStore(MemoryMobilityDraftStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityDraftStore(inner, MobilityDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilityDraft> FindMobilityDrafts(IMobilityDraftStore store, MobilityDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
