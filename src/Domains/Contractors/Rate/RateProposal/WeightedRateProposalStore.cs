namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate proposals by code.</summary>
public interface IWeightedRateProposalStore
{
    WeightedRateProposal? Find(string code);
    void Save(WeightedRateProposal item);
    bool Remove(string code);
    List<WeightedRateProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRateProposalStore(Dictionary<string, WeightedRateProposal> items, List<string> order) : IWeightedRateProposalStore
{
    public Dictionary<string, WeightedRateProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRateProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRateProposal item)
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

    public List<WeightedRateProposal> List()
    {
        var outValue = new List<WeightedRateProposal>();
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
public sealed class CachedWeightedRateProposalStore(MemoryWeightedRateProposalStore inner, WeightedRateProposalCache cache, int hits, int misses) : IWeightedRateProposalStore
{
    public MemoryWeightedRateProposalStore Inner { get; set; } = inner;
    public WeightedRateProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRateProposal? Find(string code)
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

    public void Save(WeightedRateProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRateProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate proposals. Empty fields match all.</summary>
public sealed record WeightedRateProposalQuery(string Source, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRateProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRateProposal> Apply(List<WeightedRateProposal> items)
    {
        var matched = Collections.Filter(items, (WeightedRateProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate proposal store.</summary>
public static class WeightedRateProposalStore
{
    public static IWeightedRateProposalStore NewMemoryWeightedRateProposalStore()
    {
        return new MemoryWeightedRateProposalStore(
            new Dictionary<string, WeightedRateProposal>(),
            new List<string>());
    }

    public static IWeightedRateProposalStore NewCachedWeightedRateProposalStore(MemoryWeightedRateProposalStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRateProposalStore(inner, WeightedRateProposalCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedRateProposal> FindWeightedRateProposals(IWeightedRateProposalStore store, WeightedRateProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
