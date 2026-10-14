namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay run agreements by code.</summary>
public interface ILegacyPayRunAgreementStore
{
    LegacyPayRunAgreement? Find(string code);
    void Save(LegacyPayRunAgreement item);
    bool Remove(string code);
    List<LegacyPayRunAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayRunAgreementStore(Dictionary<string, LegacyPayRunAgreement> items, List<string> order) : ILegacyPayRunAgreementStore
{
    public Dictionary<string, LegacyPayRunAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayRunAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayRunAgreement item)
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

    public List<LegacyPayRunAgreement> List()
    {
        var outValue = new List<LegacyPayRunAgreement>();
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
public sealed class CachedLegacyPayRunAgreementStore(MemoryLegacyPayRunAgreementStore inner, LegacyPayRunAgreementCache cache, int hits, int misses) : ILegacyPayRunAgreementStore
{
    public MemoryLegacyPayRunAgreementStore Inner { get; set; } = inner;
    public LegacyPayRunAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayRunAgreement? Find(string code)
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

    public void Save(LegacyPayRunAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayRunAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay run agreements. Empty fields match all.</summary>
public sealed record LegacyPayRunAgreementQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayRunAgreement item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPayRunAgreement> Apply(List<LegacyPayRunAgreement> items)
    {
        var matched = Collections.Filter(items, (LegacyPayRunAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay run agreement store.</summary>
public static class LegacyPayRunAgreementStore
{
    public static ILegacyPayRunAgreementStore NewMemoryLegacyPayRunAgreementStore()
    {
        return new MemoryLegacyPayRunAgreementStore(
            new Dictionary<string, LegacyPayRunAgreement>(),
            new List<string>());
    }

    public static ILegacyPayRunAgreementStore NewCachedLegacyPayRunAgreementStore(MemoryLegacyPayRunAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayRunAgreementStore(
            inner,
            LegacyPayRunAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyPayRunAgreement> FindLegacyPayRunAgreements(ILegacyPayRunAgreementStore store, LegacyPayRunAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
