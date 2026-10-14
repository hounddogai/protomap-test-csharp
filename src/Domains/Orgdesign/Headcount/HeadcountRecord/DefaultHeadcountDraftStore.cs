namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default headcount drafts by code.</summary>
public interface IDefaultHeadcountDraftStore
{
    DefaultHeadcountDraft? Find(string code);
    void Save(DefaultHeadcountDraft item);
    bool Remove(string code);
    List<DefaultHeadcountDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHeadcountDraftStore(Dictionary<string, DefaultHeadcountDraft> items, List<string> order) : IDefaultHeadcountDraftStore
{
    public Dictionary<string, DefaultHeadcountDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHeadcountDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHeadcountDraft item)
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

    public List<DefaultHeadcountDraft> List()
    {
        var outValue = new List<DefaultHeadcountDraft>();
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
public sealed class CachedDefaultHeadcountDraftStore(MemoryDefaultHeadcountDraftStore inner, DefaultHeadcountDraftCache cache, int hits, int misses) : IDefaultHeadcountDraftStore
{
    public MemoryDefaultHeadcountDraftStore Inner { get; set; } = inner;
    public DefaultHeadcountDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHeadcountDraft? Find(string code)
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

    public void Save(DefaultHeadcountDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHeadcountDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default headcount drafts. Empty fields match all.</summary>
public sealed record DefaultHeadcountDraftQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHeadcountDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<DefaultHeadcountDraft> Apply(List<DefaultHeadcountDraft> items)
    {
        var matched = Collections.Filter(items, (DefaultHeadcountDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default headcount draft store.</summary>
public static class DefaultHeadcountDraftStore
{
    public static IDefaultHeadcountDraftStore NewMemoryDefaultHeadcountDraftStore()
    {
        return new MemoryDefaultHeadcountDraftStore(
            new Dictionary<string, DefaultHeadcountDraft>(),
            new List<string>());
    }

    public static IDefaultHeadcountDraftStore NewCachedDefaultHeadcountDraftStore(MemoryDefaultHeadcountDraftStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHeadcountDraftStore(inner, DefaultHeadcountDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultHeadcountDraft> FindDefaultHeadcountDrafts(IDefaultHeadcountDraftStore store, DefaultHeadcountDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
