namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending offboarding drafts by code.</summary>
public interface IPendingOffboardingDraftStore
{
    PendingOffboardingDraft? Find(string code);
    void Save(PendingOffboardingDraft item);
    bool Remove(string code);
    List<PendingOffboardingDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingOffboardingDraftStore(Dictionary<string, PendingOffboardingDraft> items, List<string> order) : IPendingOffboardingDraftStore
{
    public Dictionary<string, PendingOffboardingDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingOffboardingDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingOffboardingDraft item)
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

    public List<PendingOffboardingDraft> List()
    {
        var outValue = new List<PendingOffboardingDraft>();
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
public sealed class CachedPendingOffboardingDraftStore(MemoryPendingOffboardingDraftStore inner, PendingOffboardingDraftCache cache, int hits, int misses) : IPendingOffboardingDraftStore
{
    public MemoryPendingOffboardingDraftStore Inner { get; set; } = inner;
    public PendingOffboardingDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingOffboardingDraft? Find(string code)
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

    public void Save(PendingOffboardingDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingOffboardingDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending offboarding drafts. Empty fields match all.</summary>
public sealed record PendingOffboardingDraftQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PendingOffboardingDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingOffboardingDraft> Apply(List<PendingOffboardingDraft> items)
    {
        var matched = Collections.Filter(items, (PendingOffboardingDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending offboarding draft store.</summary>
public static class PendingOffboardingDraftStore
{
    public static IPendingOffboardingDraftStore NewMemoryPendingOffboardingDraftStore()
    {
        return new MemoryPendingOffboardingDraftStore(
            new Dictionary<string, PendingOffboardingDraft>(),
            new List<string>());
    }

    public static IPendingOffboardingDraftStore NewCachedPendingOffboardingDraftStore(MemoryPendingOffboardingDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPendingOffboardingDraftStore(
            inner,
            PendingOffboardingDraftCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingOffboardingDraft> FindPendingOffboardingDrafts(IPendingOffboardingDraftStore store, PendingOffboardingDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
