namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling mobility proposals by code.</summary>
public interface IRollingMobilityProposalStore
{
    RollingMobilityProposal? Find(string code);
    void Save(RollingMobilityProposal item);
    bool Remove(string code);
    List<RollingMobilityProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingMobilityProposalStore(Dictionary<string, RollingMobilityProposal> items, List<string> order) : IRollingMobilityProposalStore
{
    public Dictionary<string, RollingMobilityProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingMobilityProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingMobilityProposal item)
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

    public List<RollingMobilityProposal> List()
    {
        var outValue = new List<RollingMobilityProposal>();
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
public sealed class CachedRollingMobilityProposalStore(MemoryRollingMobilityProposalStore inner, RollingMobilityProposalCache cache, int hits, int misses) : IRollingMobilityProposalStore
{
    public MemoryRollingMobilityProposalStore Inner { get; set; } = inner;
    public RollingMobilityProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingMobilityProposal? Find(string code)
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

    public void Save(RollingMobilityProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingMobilityProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling mobility proposals. Empty fields match all.</summary>
public sealed record RollingMobilityProposalQuery(string Source, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingMobilityProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingMobilityProposal> Apply(List<RollingMobilityProposal> items)
    {
        var matched = Collections.Filter(items, (RollingMobilityProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling mobility proposal store.</summary>
public static class RollingMobilityProposalStore
{
    public static IRollingMobilityProposalStore NewMemoryRollingMobilityProposalStore()
    {
        return new MemoryRollingMobilityProposalStore(
            new Dictionary<string, RollingMobilityProposal>(),
            new List<string>());
    }

    public static IRollingMobilityProposalStore NewCachedRollingMobilityProposalStore(MemoryRollingMobilityProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRollingMobilityProposalStore(
            inner,
            RollingMobilityProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingMobilityProposal> FindRollingMobilityProposals(IRollingMobilityProposalStore store, RollingMobilityProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
