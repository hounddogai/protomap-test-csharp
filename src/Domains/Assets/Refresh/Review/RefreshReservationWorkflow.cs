namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshReservationService(IRefreshReservationStore Store, RefreshReservationValidator Validator, IClock Clock, List<string> Journal, RefreshReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of refresh reservation workflow.</summary>
public static class RefreshReservationWorkflow
{
    public static RefreshReservationService NewRefreshReservationService(IRefreshReservationStore store, IClock clock)
    {
        return new RefreshReservationService(
            store,
            RefreshReservationRules.DefaultRefreshReservationValidator(),
            clock,
            new List<string>(),
            RefreshReservationEngine.DefaultRefreshReservationEngine());
    }
}
