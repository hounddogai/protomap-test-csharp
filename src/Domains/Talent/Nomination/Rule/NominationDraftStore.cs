namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination drafts by code.</summary>
public interface INominationDraftStore
{
    NominationDraft? Find(string code);
    void Save(NominationDraft item);
    bool Remove(string code);
    List<NominationDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationDraftStore(Dictionary<string, NominationDraft> items, List<string> order) : INominationDraftStore
{
    public Dictionary<string, NominationDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationDraft item)
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

    public List<NominationDraft> List()
    {
        var outValue = new List<NominationDraft>();
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
public sealed class CachedNominationDraftStore(MemoryNominationDraftStore inner, NominationDraftCache cache, int hits, int misses) : INominationDraftStore
{
    public MemoryNominationDraftStore Inner { get; set; } = inner;
    public NominationDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationDraft? Find(string code)
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

    public void Save(NominationDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination drafts. Empty fields match all.</summary>
public sealed record NominationDraftQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(NominationDraft item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationDraft> Apply(List<NominationDraft> items)
    {
        var matched = Collections.Filter(items, (NominationDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination draft store.</summary>
public static class NominationDraftStore
{
    public static INominationDraftStore NewMemoryNominationDraftStore()
    {
        return new MemoryNominationDraftStore(new Dictionary<string, NominationDraft>(), new List<string>());
    }

    public static INominationDraftStore NewCachedNominationDraftStore(MemoryNominationDraftStore inner, int ttl, IClock clock)
    {
        return new CachedNominationDraftStore(inner, NominationDraftCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationDraft> FindNominationDrafts(INominationDraftStore store, NominationDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
