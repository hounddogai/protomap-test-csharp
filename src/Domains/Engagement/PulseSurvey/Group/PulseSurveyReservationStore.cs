namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey reservations by code.</summary>
public interface IPulseSurveyReservationStore
{
    PulseSurveyReservation? Find(string code);
    void Save(PulseSurveyReservation item);
    bool Remove(string code);
    List<PulseSurveyReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyReservationStore(Dictionary<string, PulseSurveyReservation> items, List<string> order) : IPulseSurveyReservationStore
{
    public Dictionary<string, PulseSurveyReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyReservation item)
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

    public List<PulseSurveyReservation> List()
    {
        var outValue = new List<PulseSurveyReservation>();
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
public sealed class CachedPulseSurveyReservationStore(MemoryPulseSurveyReservationStore inner, PulseSurveyReservationCache cache, int hits, int misses) : IPulseSurveyReservationStore
{
    public MemoryPulseSurveyReservationStore Inner { get; set; } = inner;
    public PulseSurveyReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyReservation? Find(string code)
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

    public void Save(PulseSurveyReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey reservations. Empty fields match all.</summary>
public sealed record PulseSurveyReservationQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PulseSurveyReservation> Apply(List<PulseSurveyReservation> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey reservation store.</summary>
public static class PulseSurveyReservationStore
{
    public static IPulseSurveyReservationStore NewMemoryPulseSurveyReservationStore()
    {
        return new MemoryPulseSurveyReservationStore(
            new Dictionary<string, PulseSurveyReservation>(),
            new List<string>());
    }

    public static IPulseSurveyReservationStore NewCachedPulseSurveyReservationStore(MemoryPulseSurveyReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyReservationStore(
            inner,
            PulseSurveyReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PulseSurveyReservation> FindPulseSurveyReservations(IPulseSurveyReservationStore store, PulseSurveyReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
