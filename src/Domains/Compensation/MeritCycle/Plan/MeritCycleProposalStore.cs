namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle proposals by code.</summary>
public interface IMeritCycleProposalStore
{
    MeritCycleProposal? Find(string code);
    void Save(MeritCycleProposal item);
    bool Remove(string code);
    List<MeritCycleProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleProposalStore(Dictionary<string, MeritCycleProposal> items, List<string> order) : IMeritCycleProposalStore
{
    public Dictionary<string, MeritCycleProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleProposal item)
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

    public List<MeritCycleProposal> List()
    {
        var outValue = new List<MeritCycleProposal>();
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
public sealed class CachedMeritCycleProposalStore(MemoryMeritCycleProposalStore inner, MeritCycleProposalCache cache, int hits, int misses) : IMeritCycleProposalStore
{
    public MemoryMeritCycleProposalStore Inner { get; set; } = inner;
    public MeritCycleProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleProposal? Find(string code)
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

    public void Save(MeritCycleProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle proposals. Empty fields match all.</summary>
public sealed record MeritCycleProposalQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<MeritCycleProposal> Apply(List<MeritCycleProposal> items)
    {
        var matched = Collections.Filter(items, (MeritCycleProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle proposal store.</summary>
public static class MeritCycleProposalStore
{
    public static IMeritCycleProposalStore NewMemoryMeritCycleProposalStore()
    {
        return new MemoryMeritCycleProposalStore(new Dictionary<string, MeritCycleProposal>(), new List<string>());
    }

    public static IMeritCycleProposalStore NewCachedMeritCycleProposalStore(MemoryMeritCycleProposalStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleProposalStore(inner, MeritCycleProposalCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleProposal> FindMeritCycleProposals(IMeritCycleProposalStore store, MeritCycleProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
