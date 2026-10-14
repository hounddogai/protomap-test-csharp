namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination audits by code.</summary>
public interface INominationAuditStore
{
    NominationAudit? Find(string code);
    void Save(NominationAudit item);
    bool Remove(string code);
    List<NominationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationAuditStore(Dictionary<string, NominationAudit> items, List<string> order) : INominationAuditStore
{
    public Dictionary<string, NominationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationAudit item)
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

    public List<NominationAudit> List()
    {
        var outValue = new List<NominationAudit>();
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
public sealed class CachedNominationAuditStore(MemoryNominationAuditStore inner, NominationAuditCache cache, int hits, int misses) : INominationAuditStore
{
    public MemoryNominationAuditStore Inner { get; set; } = inner;
    public NominationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationAudit? Find(string code)
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

    public void Save(NominationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination audits. Empty fields match all.</summary>
public sealed record NominationAuditQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(NominationAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationAudit> Apply(List<NominationAudit> items)
    {
        var matched = Collections.Filter(items, (NominationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination audit store.</summary>
public static class NominationAuditStore
{
    public static INominationAuditStore NewMemoryNominationAuditStore()
    {
        return new MemoryNominationAuditStore(new Dictionary<string, NominationAudit>(), new List<string>());
    }

    public static INominationAuditStore NewCachedNominationAuditStore(MemoryNominationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedNominationAuditStore(inner, NominationAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<NominationAudit> FindNominationAudits(INominationAuditStore store, NominationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
