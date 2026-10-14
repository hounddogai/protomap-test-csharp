namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit drafts by code.</summary>
public interface IAuditDraftStore
{
    AuditDraft? Find(string code);
    void Save(AuditDraft item);
    bool Remove(string code);
    List<AuditDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditDraftStore(Dictionary<string, AuditDraft> items, List<string> order) : IAuditDraftStore
{
    public Dictionary<string, AuditDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditDraft item)
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

    public List<AuditDraft> List()
    {
        var outValue = new List<AuditDraft>();
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
public sealed class CachedAuditDraftStore(MemoryAuditDraftStore inner, AuditDraftCache cache, int hits, int misses) : IAuditDraftStore
{
    public MemoryAuditDraftStore Inner { get; set; } = inner;
    public AuditDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditDraft? Find(string code)
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

    public void Save(AuditDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit drafts. Empty fields match all.</summary>
public sealed record AuditDraftQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AuditDraft item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditDraft> Apply(List<AuditDraft> items)
    {
        var matched = Collections.Filter(items, (AuditDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit draft store.</summary>
public static class AuditDraftStore
{
    public static IAuditDraftStore NewMemoryAuditDraftStore()
    {
        return new MemoryAuditDraftStore(new Dictionary<string, AuditDraft>(), new List<string>());
    }

    public static IAuditDraftStore NewCachedAuditDraftStore(MemoryAuditDraftStore inner, int ttl, IClock clock)
    {
        return new CachedAuditDraftStore(inner, AuditDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AuditDraft> FindAuditDrafts(IAuditDraftStore store, AuditDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
