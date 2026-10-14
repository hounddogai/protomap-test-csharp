namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount proposals by code.</summary>
public interface IHeadcountProposalStore
{
    HeadcountProposal? Find(string code);
    void Save(HeadcountProposal item);
    bool Remove(string code);
    List<HeadcountProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountProposalStore(Dictionary<string, HeadcountProposal> items, List<string> order) : IHeadcountProposalStore
{
    public Dictionary<string, HeadcountProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountProposal item)
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

    public List<HeadcountProposal> List()
    {
        var outValue = new List<HeadcountProposal>();
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
public sealed class CachedHeadcountProposalStore(MemoryHeadcountProposalStore inner, HeadcountProposalCache cache, int hits, int misses) : IHeadcountProposalStore
{
    public MemoryHeadcountProposalStore Inner { get; set; } = inner;
    public HeadcountProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountProposal? Find(string code)
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

    public void Save(HeadcountProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount proposals. Empty fields match all.</summary>
public sealed record HeadcountProposalQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<HeadcountProposal> Apply(List<HeadcountProposal> items)
    {
        var matched = Collections.Filter(items, (HeadcountProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount proposal store.</summary>
public static class HeadcountProposalStore
{
    public static IHeadcountProposalStore NewMemoryHeadcountProposalStore()
    {
        return new MemoryHeadcountProposalStore(new Dictionary<string, HeadcountProposal>(), new List<string>());
    }

    public static IHeadcountProposalStore NewCachedHeadcountProposalStore(MemoryHeadcountProposalStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountProposalStore(inner, HeadcountProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountProposal> FindHeadcountProposals(IHeadcountProposalStore store, HeadcountProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
