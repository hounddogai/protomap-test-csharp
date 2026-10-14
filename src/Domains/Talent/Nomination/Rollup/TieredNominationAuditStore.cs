namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered nomination audits by code.</summary>
public interface ITieredNominationAuditStore
{
    TieredNominationAudit? Find(string code);
    void Save(TieredNominationAudit item);
    bool Remove(string code);
    List<TieredNominationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredNominationAuditStore(Dictionary<string, TieredNominationAudit> items, List<string> order) : ITieredNominationAuditStore
{
    public Dictionary<string, TieredNominationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredNominationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredNominationAudit item)
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

    public List<TieredNominationAudit> List()
    {
        var outValue = new List<TieredNominationAudit>();
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
public sealed class CachedTieredNominationAuditStore(MemoryTieredNominationAuditStore inner, TieredNominationAuditCache cache, int hits, int misses) : ITieredNominationAuditStore
{
    public MemoryTieredNominationAuditStore Inner { get; set; } = inner;
    public TieredNominationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredNominationAudit? Find(string code)
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

    public void Save(TieredNominationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredNominationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered nomination audits. Empty fields match all.</summary>
public sealed record TieredNominationAuditQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredNominationAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredNominationAudit> Apply(List<TieredNominationAudit> items)
    {
        var matched = Collections.Filter(items, (TieredNominationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered nomination audit store.</summary>
public static class TieredNominationAuditStore
{
    public static ITieredNominationAuditStore NewMemoryTieredNominationAuditStore()
    {
        return new MemoryTieredNominationAuditStore(
            new Dictionary<string, TieredNominationAudit>(),
            new List<string>());
    }

    public static ITieredNominationAuditStore NewCachedTieredNominationAuditStore(MemoryTieredNominationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedTieredNominationAuditStore(
            inner,
            TieredNominationAuditCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredNominationAudit> FindTieredNominationAudits(ITieredNominationAuditStore store, TieredNominationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
