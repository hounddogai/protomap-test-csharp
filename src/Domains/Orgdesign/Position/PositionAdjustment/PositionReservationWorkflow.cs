namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionReservationCoordinator(IPositionReservationStore Store, PositionReservationValidator Validator, IClock Clock, List<string> Journal, PositionReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position reservation {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of position reservation workflow.</summary>
public static class PositionReservationWorkflow
{
    public static PositionReservationCoordinator NewPositionReservationCoordinator(IPositionReservationStore store, IClock clock)
    {
        return new PositionReservationCoordinator(
            store,
            PositionReservationRules.DefaultPositionReservationValidator(),
            clock,
            new List<string>(),
            PositionReservationEngine.DefaultPositionReservationEngine());
    }
}
