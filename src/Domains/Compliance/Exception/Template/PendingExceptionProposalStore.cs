namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending exception proposals by code.</summary>
public interface IPendingExceptionProposalStore
{
    PendingExceptionProposal? Find(string code);
    void Save(PendingExceptionProposal item);
    bool Remove(string code);
    List<PendingExceptionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingExceptionProposalStore(Dictionary<string, PendingExceptionProposal> items, List<string> order) : IPendingExceptionProposalStore
{
    public Dictionary<string, PendingExceptionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingExceptionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingExceptionProposal item)
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

    public List<PendingExceptionProposal> List()
    {
        var outValue = new List<PendingExceptionProposal>();
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
public sealed class CachedPendingExceptionProposalStore(MemoryPendingExceptionProposalStore inner, PendingExceptionProposalCache cache, int hits, int misses) : IPendingExceptionProposalStore
{
    public MemoryPendingExceptionProposalStore Inner { get; set; } = inner;
    public PendingExceptionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingExceptionProposal? Find(string code)
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

    public void Save(PendingExceptionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingExceptionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending exception proposals. Empty fields match all.</summary>
public sealed record PendingExceptionProposalQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PendingExceptionProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PendingExceptionProposal> Apply(List<PendingExceptionProposal> items)
    {
        var matched = Collections.Filter(items, (PendingExceptionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending exception proposal store.</summary>
public static class PendingExceptionProposalStore
{
    public static IPendingExceptionProposalStore NewMemoryPendingExceptionProposalStore()
    {
        return new MemoryPendingExceptionProposalStore(
            new Dictionary<string, PendingExceptionProposal>(),
            new List<string>());
    }

    public static IPendingExceptionProposalStore NewCachedPendingExceptionProposalStore(MemoryPendingExceptionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPendingExceptionProposalStore(
            inner,
            PendingExceptionProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingExceptionProposal> FindPendingExceptionProposals(IPendingExceptionProposalStore store, PendingExceptionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
