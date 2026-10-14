namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk proposals by code.</summary>
public interface IDeskProposalStore
{
    DeskProposal? Find(string code);
    void Save(DeskProposal item);
    bool Remove(string code);
    List<DeskProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskProposalStore(Dictionary<string, DeskProposal> items, List<string> order) : IDeskProposalStore
{
    public Dictionary<string, DeskProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskProposal item)
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

    public List<DeskProposal> List()
    {
        var outValue = new List<DeskProposal>();
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
public sealed class CachedDeskProposalStore(MemoryDeskProposalStore inner, DeskProposalCache cache, int hits, int misses) : IDeskProposalStore
{
    public MemoryDeskProposalStore Inner { get; set; } = inner;
    public DeskProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskProposal? Find(string code)
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

    public void Save(DeskProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk proposals. Empty fields match all.</summary>
public sealed record DeskProposalQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeskProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskProposal> Apply(List<DeskProposal> items)
    {
        var matched = Collections.Filter(items, (DeskProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk proposal store.</summary>
public static class DeskProposalStore
{
    public static IDeskProposalStore NewMemoryDeskProposalStore()
    {
        return new MemoryDeskProposalStore(new Dictionary<string, DeskProposal>(), new List<string>());
    }

    public static IDeskProposalStore NewCachedDeskProposalStore(MemoryDeskProposalStore inner, int ttl, IClock clock)
    {
        return new CachedDeskProposalStore(inner, DeskProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskProposal> FindDeskProposals(IDeskProposalStore store, DeskProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
