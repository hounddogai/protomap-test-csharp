namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile reservations by code.</summary>
public interface ISkillProfileReservationStore
{
    SkillProfileReservation? Find(string code);
    void Save(SkillProfileReservation item);
    bool Remove(string code);
    List<SkillProfileReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileReservationStore(Dictionary<string, SkillProfileReservation> items, List<string> order) : ISkillProfileReservationStore
{
    public Dictionary<string, SkillProfileReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileReservation item)
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

    public List<SkillProfileReservation> List()
    {
        var outValue = new List<SkillProfileReservation>();
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
public sealed class CachedSkillProfileReservationStore(MemorySkillProfileReservationStore inner, SkillProfileReservationCache cache, int hits, int misses) : ISkillProfileReservationStore
{
    public MemorySkillProfileReservationStore Inner { get; set; } = inner;
    public SkillProfileReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileReservation? Find(string code)
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

    public void Save(SkillProfileReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile reservations. Empty fields match all.</summary>
public sealed record SkillProfileReservationQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileReservation> Apply(List<SkillProfileReservation> items)
    {
        var matched = Collections.Filter(items, (SkillProfileReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile reservation store.</summary>
public static class SkillProfileReservationStore
{
    public static ISkillProfileReservationStore NewMemorySkillProfileReservationStore()
    {
        return new MemorySkillProfileReservationStore(
            new Dictionary<string, SkillProfileReservation>(),
            new List<string>());
    }

    public static ISkillProfileReservationStore NewCachedSkillProfileReservationStore(MemorySkillProfileReservationStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileReservationStore(
            inner,
            SkillProfileReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SkillProfileReservation> FindSkillProfileReservations(ISkillProfileReservationStore store, SkillProfileReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
