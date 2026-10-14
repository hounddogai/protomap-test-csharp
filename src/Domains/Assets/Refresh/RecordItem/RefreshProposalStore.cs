namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh proposals by code.</summary>
public interface IRefreshProposalStore
{
    RefreshProposal? Find(string code);
    void Save(RefreshProposal item);
    bool Remove(string code);
    List<RefreshProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshProposalStore(Dictionary<string, RefreshProposal> items, List<string> order) : IRefreshProposalStore
{
    public Dictionary<string, RefreshProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshProposal item)
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

    public List<RefreshProposal> List()
    {
        var outValue = new List<RefreshProposal>();
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
public sealed class CachedRefreshProposalStore(MemoryRefreshProposalStore inner, RefreshProposalCache cache, int hits, int misses) : IRefreshProposalStore
{
    public MemoryRefreshProposalStore Inner { get; set; } = inner;
    public RefreshProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshProposal? Find(string code)
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

    public void Save(RefreshProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh proposals. Empty fields match all.</summary>
public sealed record RefreshProposalQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshProposal> Apply(List<RefreshProposal> items)
    {
        var matched = Collections.Filter(items, (RefreshProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh proposal store.</summary>
public static class RefreshProposalStore
{
    public static IRefreshProposalStore NewMemoryRefreshProposalStore()
    {
        return new MemoryRefreshProposalStore(new Dictionary<string, RefreshProposal>(), new List<string>());
    }

    public static IRefreshProposalStore NewCachedRefreshProposalStore(MemoryRefreshProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshProposalStore(inner, RefreshProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshProposal> FindRefreshProposals(IRefreshProposalStore store, RefreshProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
