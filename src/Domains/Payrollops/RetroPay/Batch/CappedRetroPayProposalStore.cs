namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped retro pay proposals by code.</summary>
public interface ICappedRetroPayProposalStore
{
    CappedRetroPayProposal? Find(string code);
    void Save(CappedRetroPayProposal item);
    bool Remove(string code);
    List<CappedRetroPayProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRetroPayProposalStore(Dictionary<string, CappedRetroPayProposal> items, List<string> order) : ICappedRetroPayProposalStore
{
    public Dictionary<string, CappedRetroPayProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRetroPayProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRetroPayProposal item)
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

    public List<CappedRetroPayProposal> List()
    {
        var outValue = new List<CappedRetroPayProposal>();
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
public sealed class CachedCappedRetroPayProposalStore(MemoryCappedRetroPayProposalStore inner, CappedRetroPayProposalCache cache, int hits, int misses) : ICappedRetroPayProposalStore
{
    public MemoryCappedRetroPayProposalStore Inner { get; set; } = inner;
    public CappedRetroPayProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRetroPayProposal? Find(string code)
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

    public void Save(CappedRetroPayProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRetroPayProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped retro pay proposals. Empty fields match all.</summary>
public sealed record CappedRetroPayProposalQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRetroPayProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRetroPayProposal> Apply(List<CappedRetroPayProposal> items)
    {
        var matched = Collections.Filter(items, (CappedRetroPayProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped retro pay proposal store.</summary>
public static class CappedRetroPayProposalStore
{
    public static ICappedRetroPayProposalStore NewMemoryCappedRetroPayProposalStore()
    {
        return new MemoryCappedRetroPayProposalStore(
            new Dictionary<string, CappedRetroPayProposal>(),
            new List<string>());
    }

    public static ICappedRetroPayProposalStore NewCachedCappedRetroPayProposalStore(MemoryCappedRetroPayProposalStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRetroPayProposalStore(
            inner,
            CappedRetroPayProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedRetroPayProposal> FindCappedRetroPayProposals(ICappedRetroPayProposalStore store, CappedRetroPayProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
