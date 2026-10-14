namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition templates by code.</summary>
public interface IRecognitionTemplateStore
{
    RecognitionTemplate? Find(string code);
    void Save(RecognitionTemplate item);
    bool Remove(string code);
    List<RecognitionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionTemplateStore(Dictionary<string, RecognitionTemplate> items, List<string> order) : IRecognitionTemplateStore
{
    public Dictionary<string, RecognitionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionTemplate item)
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

    public List<RecognitionTemplate> List()
    {
        var outValue = new List<RecognitionTemplate>();
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
public sealed class CachedRecognitionTemplateStore(MemoryRecognitionTemplateStore inner, RecognitionTemplateCache cache, int hits, int misses) : IRecognitionTemplateStore
{
    public MemoryRecognitionTemplateStore Inner { get; set; } = inner;
    public RecognitionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionTemplate? Find(string code)
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

    public void Save(RecognitionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition templates. Empty fields match all.</summary>
public sealed record RecognitionTemplateQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionTemplate item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionTemplate> Apply(List<RecognitionTemplate> items)
    {
        var matched = Collections.Filter(items, (RecognitionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition template store.</summary>
public static class RecognitionTemplateStore
{
    public static IRecognitionTemplateStore NewMemoryRecognitionTemplateStore()
    {
        return new MemoryRecognitionTemplateStore(new Dictionary<string, RecognitionTemplate>(), new List<string>());
    }

    public static IRecognitionTemplateStore NewCachedRecognitionTemplateStore(MemoryRecognitionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionTemplateStore(inner, RecognitionTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RecognitionTemplate> FindRecognitionTemplates(IRecognitionTemplateStore store, RecognitionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
