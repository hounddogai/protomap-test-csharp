namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default adjustment agreements by code.</summary>
public interface IDefaultAdjustmentAgreementStore
{
    DefaultAdjustmentAgreement? Find(string code);
    void Save(DefaultAdjustmentAgreement item);
    bool Remove(string code);
    List<DefaultAdjustmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAdjustmentAgreementStore(Dictionary<string, DefaultAdjustmentAgreement> items, List<string> order) : IDefaultAdjustmentAgreementStore
{
    public Dictionary<string, DefaultAdjustmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAdjustmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAdjustmentAgreement item)
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

    public List<DefaultAdjustmentAgreement> List()
    {
        var outValue = new List<DefaultAdjustmentAgreement>();
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
public sealed class CachedDefaultAdjustmentAgreementStore(MemoryDefaultAdjustmentAgreementStore inner, DefaultAdjustmentAgreementCache cache, int hits, int misses) : IDefaultAdjustmentAgreementStore
{
    public MemoryDefaultAdjustmentAgreementStore Inner { get; set; } = inner;
    public DefaultAdjustmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAdjustmentAgreement? Find(string code)
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

    public void Save(DefaultAdjustmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAdjustmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default adjustment agreements. Empty fields match all.</summary>
public sealed record DefaultAdjustmentAgreementQuery(string Team, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAdjustmentAgreement item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAdjustmentAgreement> Apply(List<DefaultAdjustmentAgreement> items)
    {
        var matched = Collections.Filter(items, (DefaultAdjustmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default adjustment agreement store.</summary>
public static class DefaultAdjustmentAgreementStore
{
    public static IDefaultAdjustmentAgreementStore NewMemoryDefaultAdjustmentAgreementStore()
    {
        return new MemoryDefaultAdjustmentAgreementStore(
            new Dictionary<string, DefaultAdjustmentAgreement>(),
            new List<string>());
    }

    public static IDefaultAdjustmentAgreementStore NewCachedDefaultAdjustmentAgreementStore(MemoryDefaultAdjustmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAdjustmentAgreementStore(
            inner,
            DefaultAdjustmentAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAdjustmentAgreement> FindDefaultAdjustmentAgreements(IDefaultAdjustmentAgreementStore store, DefaultAdjustmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
