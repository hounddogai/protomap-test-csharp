namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary policy ack drafts by code.</summary>
public interface IPrimaryPolicyAckDraftStore
{
    PrimaryPolicyAckDraft? Find(string code);
    void Save(PrimaryPolicyAckDraft item);
    bool Remove(string code);
    List<PrimaryPolicyAckDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPolicyAckDraftStore(Dictionary<string, PrimaryPolicyAckDraft> items, List<string> order) : IPrimaryPolicyAckDraftStore
{
    public Dictionary<string, PrimaryPolicyAckDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPolicyAckDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPolicyAckDraft item)
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

    public List<PrimaryPolicyAckDraft> List()
    {
        var outValue = new List<PrimaryPolicyAckDraft>();
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
public sealed class CachedPrimaryPolicyAckDraftStore(MemoryPrimaryPolicyAckDraftStore inner, PrimaryPolicyAckDraftCache cache, int hits, int misses) : IPrimaryPolicyAckDraftStore
{
    public MemoryPrimaryPolicyAckDraftStore Inner { get; set; } = inner;
    public PrimaryPolicyAckDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPolicyAckDraft? Find(string code)
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

    public void Save(PrimaryPolicyAckDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPolicyAckDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary policy ack drafts. Empty fields match all.</summary>
public sealed record PrimaryPolicyAckDraftQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPolicyAckDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPolicyAckDraft> Apply(List<PrimaryPolicyAckDraft> items)
    {
        var matched = Collections.Filter(items, (PrimaryPolicyAckDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary policy ack draft store.</summary>
public static class PrimaryPolicyAckDraftStore
{
    public static IPrimaryPolicyAckDraftStore NewMemoryPrimaryPolicyAckDraftStore()
    {
        return new MemoryPrimaryPolicyAckDraftStore(
            new Dictionary<string, PrimaryPolicyAckDraft>(),
            new List<string>());
    }

    public static IPrimaryPolicyAckDraftStore NewCachedPrimaryPolicyAckDraftStore(MemoryPrimaryPolicyAckDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPolicyAckDraftStore(inner, PrimaryPolicyAckDraftCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryPolicyAckDraft> FindPrimaryPolicyAckDrafts(IPrimaryPolicyAckDraftStore store, PrimaryPolicyAckDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
