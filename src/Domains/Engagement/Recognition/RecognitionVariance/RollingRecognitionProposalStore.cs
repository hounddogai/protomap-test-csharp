namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling recognition proposals by code.</summary>
public interface IRollingRecognitionProposalStore
{
    RollingRecognitionProposal? Find(string code);
    void Save(RollingRecognitionProposal item);
    bool Remove(string code);
    List<RollingRecognitionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRecognitionProposalStore(Dictionary<string, RollingRecognitionProposal> items, List<string> order) : IRollingRecognitionProposalStore
{
    public Dictionary<string, RollingRecognitionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRecognitionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRecognitionProposal item)
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

    public List<RollingRecognitionProposal> List()
    {
        var outValue = new List<RollingRecognitionProposal>();
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
public sealed class CachedRollingRecognitionProposalStore(MemoryRollingRecognitionProposalStore inner, RollingRecognitionProposalCache cache, int hits, int misses) : IRollingRecognitionProposalStore
{
    public MemoryRollingRecognitionProposalStore Inner { get; set; } = inner;
    public RollingRecognitionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRecognitionProposal? Find(string code)
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

    public void Save(RollingRecognitionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRecognitionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling recognition proposals. Empty fields match all.</summary>
public sealed record RollingRecognitionProposalQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRecognitionProposal item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRecognitionProposal> Apply(List<RollingRecognitionProposal> items)
    {
        var matched = Collections.Filter(items, (RollingRecognitionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling recognition proposal store.</summary>
public static class RollingRecognitionProposalStore
{
    public static IRollingRecognitionProposalStore NewMemoryRollingRecognitionProposalStore()
    {
        return new MemoryRollingRecognitionProposalStore(
            new Dictionary<string, RollingRecognitionProposal>(),
            new List<string>());
    }

    public static IRollingRecognitionProposalStore NewCachedRollingRecognitionProposalStore(MemoryRollingRecognitionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRecognitionProposalStore(
            inner,
            RollingRecognitionProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingRecognitionProposal> FindRollingRecognitionProposals(IRollingRecognitionProposalStore store, RollingRecognitionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
