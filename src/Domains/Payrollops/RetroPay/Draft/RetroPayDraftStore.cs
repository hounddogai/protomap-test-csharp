namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay drafts by code.</summary>
public interface IRetroPayDraftStore
{
    RetroPayDraft? Find(string code);
    void Save(RetroPayDraft item);
    bool Remove(string code);
    List<RetroPayDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayDraftStore(Dictionary<string, RetroPayDraft> items, List<string> order) : IRetroPayDraftStore
{
    public Dictionary<string, RetroPayDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayDraft item)
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

    public List<RetroPayDraft> List()
    {
        var outValue = new List<RetroPayDraft>();
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
public sealed class CachedRetroPayDraftStore(MemoryRetroPayDraftStore inner, RetroPayDraftCache cache, int hits, int misses) : IRetroPayDraftStore
{
    public MemoryRetroPayDraftStore Inner { get; set; } = inner;
    public RetroPayDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayDraft? Find(string code)
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

    public void Save(RetroPayDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay drafts. Empty fields match all.</summary>
public sealed record RetroPayDraftQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayDraft> Apply(List<RetroPayDraft> items)
    {
        var matched = Collections.Filter(items, (RetroPayDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay draft store.</summary>
public static class RetroPayDraftStore
{
    public static IRetroPayDraftStore NewMemoryRetroPayDraftStore()
    {
        return new MemoryRetroPayDraftStore(new Dictionary<string, RetroPayDraft>(), new List<string>());
    }

    public static IRetroPayDraftStore NewCachedRetroPayDraftStore(MemoryRetroPayDraftStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayDraftStore(inner, RetroPayDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayDraft> FindRetroPayDrafts(IRetroPayDraftStore store, RetroPayDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
