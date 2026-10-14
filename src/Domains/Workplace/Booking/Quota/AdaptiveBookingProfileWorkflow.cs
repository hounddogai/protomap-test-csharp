namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive booking profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveBookingProfileWorkflow(IAdaptiveBookingProfileStore Store, AdaptiveBookingProfileValidator Validator, IClock Clock, List<string> Journal)
{
    public static AdaptiveBookingProfileWorkflow NewAdaptiveBookingProfileWorkflow(IAdaptiveBookingProfileStore store, IClock clock)
    {
        return new AdaptiveBookingProfileWorkflow(
            store,
            AdaptiveBookingProfileRules.DefaultAdaptiveBookingProfileValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive booking profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBookingProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive booking profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBookingProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int ReviewStale(int cutoff)
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
