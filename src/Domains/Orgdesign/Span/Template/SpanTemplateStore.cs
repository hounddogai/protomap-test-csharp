namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span templates by code.</summary>
public interface ISpanTemplateStore
{
    SpanTemplate? Find(string code);
    void Save(SpanTemplate item);
    bool Remove(string code);
    List<SpanTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanTemplateStore(Dictionary<string, SpanTemplate> items, List<string> order) : ISpanTemplateStore
{
    public Dictionary<string, SpanTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanTemplate item)
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

    public List<SpanTemplate> List()
    {
        var outValue = new List<SpanTemplate>();
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
public sealed class CachedSpanTemplateStore(MemorySpanTemplateStore inner, SpanTemplateCache cache, int hits, int misses) : ISpanTemplateStore
{
    public MemorySpanTemplateStore Inner { get; set; } = inner;
    public SpanTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanTemplate? Find(string code)
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

    public void Save(SpanTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span templates. Empty fields match all.</summary>
public sealed record SpanTemplateQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SpanTemplate item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanTemplate> Apply(List<SpanTemplate> items)
    {
        var matched = Collections.Filter(items, (SpanTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span template store.</summary>
public static class SpanTemplateStore
{
    public static ISpanTemplateStore NewMemorySpanTemplateStore()
    {
        return new MemorySpanTemplateStore(new Dictionary<string, SpanTemplate>(), new List<string>());
    }

    public static ISpanTemplateStore NewCachedSpanTemplateStore(MemorySpanTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedSpanTemplateStore(inner, SpanTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanTemplate> FindSpanTemplates(ISpanTemplateStore store, SpanTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
