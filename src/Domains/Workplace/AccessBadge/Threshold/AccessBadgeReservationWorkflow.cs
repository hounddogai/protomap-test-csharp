namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeReservationWorkflow(IAccessBadgeReservationStore Store, AccessBadgeReservationValidator Validator, IClock Clock, List<string> Journal, AccessBadgeReservationEngine Engine)
{
    public static AccessBadgeReservationWorkflow NewAccessBadgeReservationWorkflow(IAccessBadgeReservationStore store, IClock clock)
    {
        return new AccessBadgeReservationWorkflow(
            store,
            AccessBadgeReservationRules.DefaultAccessBadgeReservationValidator(),
            clock,
            new List<string>(),
            AccessBadgeReservationEngine.DefaultAccessBadgeReservationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge reservation {code} not found");
        }
        var result = Engine.Compute(item);
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
