namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional booking preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalBookingPreferenceWorkflow(IRegionalBookingPreferenceStore Store, RegionalBookingPreferenceValidator Validator, IClock Clock, List<string> Journal, RegionalBookingPreferenceEngine Engine)
{
    public static RegionalBookingPreferenceWorkflow NewRegionalBookingPreferenceWorkflow(IRegionalBookingPreferenceStore store, IClock clock)
    {
        return new RegionalBookingPreferenceWorkflow(
            store,
            RegionalBookingPreferenceRules.DefaultRegionalBookingPreferenceValidator(),
            clock,
            new List<string>(),
            RegionalBookingPreferenceEngine.DefaultRegionalBookingPreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional booking preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalBookingPreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional booking preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalBookingPreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional booking preference {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}
