namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle agreements by code.</summary>
public interface IMeritCycleAgreementStore
{
    MeritCycleAgreement? Find(string code);
    void Save(MeritCycleAgreement item);
    bool Remove(string code);
    List<MeritCycleAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleAgreementStore(Dictionary<string, MeritCycleAgreement> items, List<string> order) : IMeritCycleAgreementStore
{
    public Dictionary<string, MeritCycleAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleAgreement item)
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

    public List<MeritCycleAgreement> List()
    {
        var outValue = new List<MeritCycleAgreement>();
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
public sealed class CachedMeritCycleAgreementStore(MemoryMeritCycleAgreementStore inner, MeritCycleAgreementCache cache, int hits, int misses) : IMeritCycleAgreementStore
{
    public MemoryMeritCycleAgreementStore Inner { get; set; } = inner;
    public MeritCycleAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleAgreement? Find(string code)
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

    public void Save(MeritCycleAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle agreements. Empty fields match all.</summary>
public sealed record MeritCycleAgreementQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleAgreement> Apply(List<MeritCycleAgreement> items)
    {
        var matched = Collections.Filter(items, (MeritCycleAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle agreement store.</summary>
public static class MeritCycleAgreementStore
{
    public static IMeritCycleAgreementStore NewMemoryMeritCycleAgreementStore()
    {
        return new MemoryMeritCycleAgreementStore(new Dictionary<string, MeritCycleAgreement>(), new List<string>());
    }

    public static IMeritCycleAgreementStore NewCachedMeritCycleAgreementStore(MemoryMeritCycleAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleAgreementStore(inner, MeritCycleAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleAgreement> FindMeritCycleAgreements(IMeritCycleAgreementStore store, MeritCycleAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
