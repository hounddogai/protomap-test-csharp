namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary position proposals by code.</summary>
public interface IPrimaryPositionProposalStore
{
    PrimaryPositionProposal? Find(string code);
    void Save(PrimaryPositionProposal item);
    bool Remove(string code);
    List<PrimaryPositionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPositionProposalStore(Dictionary<string, PrimaryPositionProposal> items, List<string> order) : IPrimaryPositionProposalStore
{
    public Dictionary<string, PrimaryPositionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPositionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPositionProposal item)
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

    public List<PrimaryPositionProposal> List()
    {
        var outValue = new List<PrimaryPositionProposal>();
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
public sealed class CachedPrimaryPositionProposalStore(MemoryPrimaryPositionProposalStore inner, PrimaryPositionProposalCache cache, int hits, int misses) : IPrimaryPositionProposalStore
{
    public MemoryPrimaryPositionProposalStore Inner { get; set; } = inner;
    public PrimaryPositionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPositionProposal? Find(string code)
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

    public void Save(PrimaryPositionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPositionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary position proposals. Empty fields match all.</summary>
public sealed record PrimaryPositionProposalQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPositionProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPositionProposal> Apply(List<PrimaryPositionProposal> items)
    {
        var matched = Collections.Filter(items, (PrimaryPositionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary position proposal store.</summary>
public static class PrimaryPositionProposalStore
{
    public static IPrimaryPositionProposalStore NewMemoryPrimaryPositionProposalStore()
    {
        return new MemoryPrimaryPositionProposalStore(
            new Dictionary<string, PrimaryPositionProposal>(),
            new List<string>());
    }

    public static IPrimaryPositionProposalStore NewCachedPrimaryPositionProposalStore(MemoryPrimaryPositionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPositionProposalStore(
            inner,
            PrimaryPositionProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPositionProposal> FindPrimaryPositionProposals(IPrimaryPositionProposalStore store, PrimaryPositionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
