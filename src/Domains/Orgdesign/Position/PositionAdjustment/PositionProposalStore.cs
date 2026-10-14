namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position proposals by code.</summary>
public interface IPositionProposalStore
{
    PositionProposal? Find(string code);
    void Save(PositionProposal item);
    bool Remove(string code);
    List<PositionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionProposalStore(Dictionary<string, PositionProposal> items, List<string> order) : IPositionProposalStore
{
    public Dictionary<string, PositionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionProposal item)
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

    public List<PositionProposal> List()
    {
        var outValue = new List<PositionProposal>();
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
public sealed class CachedPositionProposalStore(MemoryPositionProposalStore inner, PositionProposalCache cache, int hits, int misses) : IPositionProposalStore
{
    public MemoryPositionProposalStore Inner { get; set; } = inner;
    public PositionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionProposal? Find(string code)
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

    public void Save(PositionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position proposals. Empty fields match all.</summary>
public sealed record PositionProposalQuery(string Segment, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PositionProposal item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionProposal> Apply(List<PositionProposal> items)
    {
        var matched = Collections.Filter(items, (PositionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position proposal store.</summary>
public static class PositionProposalStore
{
    public static IPositionProposalStore NewMemoryPositionProposalStore()
    {
        return new MemoryPositionProposalStore(new Dictionary<string, PositionProposal>(), new List<string>());
    }

    public static IPositionProposalStore NewCachedPositionProposalStore(MemoryPositionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPositionProposalStore(inner, PositionProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionProposal> FindPositionProposals(IPositionProposalStore store, PositionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
