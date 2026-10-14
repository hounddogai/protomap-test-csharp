namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition proposals by code.</summary>
public interface IRecognitionProposalStore
{
    RecognitionProposal? Find(string code);
    void Save(RecognitionProposal item);
    bool Remove(string code);
    List<RecognitionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionProposalStore(Dictionary<string, RecognitionProposal> items, List<string> order) : IRecognitionProposalStore
{
    public Dictionary<string, RecognitionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionProposal item)
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

    public List<RecognitionProposal> List()
    {
        var outValue = new List<RecognitionProposal>();
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
public sealed class CachedRecognitionProposalStore(MemoryRecognitionProposalStore inner, RecognitionProposalCache cache, int hits, int misses) : IRecognitionProposalStore
{
    public MemoryRecognitionProposalStore Inner { get; set; } = inner;
    public RecognitionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionProposal? Find(string code)
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

    public void Save(RecognitionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition proposals. Empty fields match all.</summary>
public sealed record RecognitionProposalQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RecognitionProposal> Apply(List<RecognitionProposal> items)
    {
        var matched = Collections.Filter(items, (RecognitionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition proposal store.</summary>
public static class RecognitionProposalStore
{
    public static IRecognitionProposalStore NewMemoryRecognitionProposalStore()
    {
        return new MemoryRecognitionProposalStore(new Dictionary<string, RecognitionProposal>(), new List<string>());
    }

    public static IRecognitionProposalStore NewCachedRecognitionProposalStore(MemoryRecognitionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionProposalStore(inner, RecognitionProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RecognitionProposal> FindRecognitionProposals(IRecognitionProposalStore store, RecognitionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
