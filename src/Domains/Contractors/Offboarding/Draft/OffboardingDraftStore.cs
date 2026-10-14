namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding drafts by code.</summary>
public interface IOffboardingDraftStore
{
    OffboardingDraft? Find(string code);
    void Save(OffboardingDraft item);
    bool Remove(string code);
    List<OffboardingDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingDraftStore(Dictionary<string, OffboardingDraft> items, List<string> order) : IOffboardingDraftStore
{
    public Dictionary<string, OffboardingDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingDraft item)
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

    public List<OffboardingDraft> List()
    {
        var outValue = new List<OffboardingDraft>();
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
public sealed class CachedOffboardingDraftStore(MemoryOffboardingDraftStore inner, OffboardingDraftCache cache, int hits, int misses) : IOffboardingDraftStore
{
    public MemoryOffboardingDraftStore Inner { get; set; } = inner;
    public OffboardingDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingDraft? Find(string code)
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

    public void Save(OffboardingDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding drafts. Empty fields match all.</summary>
public sealed record OffboardingDraftQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingDraft> Apply(List<OffboardingDraft> items)
    {
        var matched = Collections.Filter(items, (OffboardingDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding draft store.</summary>
public static class OffboardingDraftStore
{
    public static IOffboardingDraftStore NewMemoryOffboardingDraftStore()
    {
        return new MemoryOffboardingDraftStore(new Dictionary<string, OffboardingDraft>(), new List<string>());
    }

    public static IOffboardingDraftStore NewCachedOffboardingDraftStore(MemoryOffboardingDraftStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingDraftStore(inner, OffboardingDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OffboardingDraft> FindOffboardingDrafts(IOffboardingDraftStore store, OffboardingDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
