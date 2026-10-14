namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered position proposals by code.</summary>
public interface ITieredPositionProposalStore
{
    TieredPositionProposal? Find(string code);
    void Save(TieredPositionProposal item);
    bool Remove(string code);
    List<TieredPositionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPositionProposalStore(Dictionary<string, TieredPositionProposal> items, List<string> order) : ITieredPositionProposalStore
{
    public Dictionary<string, TieredPositionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPositionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPositionProposal item)
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

    public List<TieredPositionProposal> List()
    {
        var outValue = new List<TieredPositionProposal>();
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
public sealed class CachedTieredPositionProposalStore(MemoryTieredPositionProposalStore inner, TieredPositionProposalCache cache, int hits, int misses) : ITieredPositionProposalStore
{
    public MemoryTieredPositionProposalStore Inner { get; set; } = inner;
    public TieredPositionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPositionProposal? Find(string code)
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

    public void Save(TieredPositionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPositionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered position proposals. Empty fields match all.</summary>
public sealed record TieredPositionProposalQuery(string Department, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPositionProposal item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPositionProposal> Apply(List<TieredPositionProposal> items)
    {
        var matched = Collections.Filter(items, (TieredPositionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered position proposal store.</summary>
public static class TieredPositionProposalStore
{
    public static ITieredPositionProposalStore NewMemoryTieredPositionProposalStore()
    {
        return new MemoryTieredPositionProposalStore(
            new Dictionary<string, TieredPositionProposal>(),
            new List<string>());
    }

    public static ITieredPositionProposalStore NewCachedTieredPositionProposalStore(MemoryTieredPositionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPositionProposalStore(
            inner,
            TieredPositionProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredPositionProposal> FindTieredPositionProposals(ITieredPositionProposalStore store, TieredPositionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
