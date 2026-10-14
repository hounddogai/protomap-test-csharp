namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual bonus pool proposals by code.</summary>
public interface IManualBonusPoolProposalStore
{
    ManualBonusPoolProposal? Find(string code);
    void Save(ManualBonusPoolProposal item);
    bool Remove(string code);
    List<ManualBonusPoolProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBonusPoolProposalStore(Dictionary<string, ManualBonusPoolProposal> items, List<string> order) : IManualBonusPoolProposalStore
{
    public Dictionary<string, ManualBonusPoolProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBonusPoolProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBonusPoolProposal item)
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

    public List<ManualBonusPoolProposal> List()
    {
        var outValue = new List<ManualBonusPoolProposal>();
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
public sealed class CachedManualBonusPoolProposalStore(MemoryManualBonusPoolProposalStore inner, ManualBonusPoolProposalCache cache, int hits, int misses) : IManualBonusPoolProposalStore
{
    public MemoryManualBonusPoolProposalStore Inner { get; set; } = inner;
    public ManualBonusPoolProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBonusPoolProposal? Find(string code)
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

    public void Save(ManualBonusPoolProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBonusPoolProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual bonus pool proposals. Empty fields match all.</summary>
public sealed record ManualBonusPoolProposalQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBonusPoolProposal item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBonusPoolProposal> Apply(List<ManualBonusPoolProposal> items)
    {
        var matched = Collections.Filter(items, (ManualBonusPoolProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual bonus pool proposal store.</summary>
public static class ManualBonusPoolProposalStore
{
    public static IManualBonusPoolProposalStore NewMemoryManualBonusPoolProposalStore()
    {
        return new MemoryManualBonusPoolProposalStore(
            new Dictionary<string, ManualBonusPoolProposal>(),
            new List<string>());
    }

    public static IManualBonusPoolProposalStore NewCachedManualBonusPoolProposalStore(MemoryManualBonusPoolProposalStore inner, int ttl, IClock clock)
    {
        return new CachedManualBonusPoolProposalStore(
            inner,
            ManualBonusPoolProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualBonusPoolProposal> FindManualBonusPoolProposals(IManualBonusPoolProposalStore store, ManualBonusPoolProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
