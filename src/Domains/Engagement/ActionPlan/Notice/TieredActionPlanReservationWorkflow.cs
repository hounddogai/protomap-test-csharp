namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered action plan reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredActionPlanReservationCoordinator(ITieredActionPlanReservationStore Store, TieredActionPlanReservationValidator Validator, IClock Clock, List<string> Journal, TieredActionPlanReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered action plan reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredActionPlanReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered action plan reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredActionPlanReservationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered action plan reservation {code} not found");
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

/// <summary>Functions of tiered action plan reservation workflow.</summary>
public static class TieredActionPlanReservationWorkflow
{
    public static TieredActionPlanReservationCoordinator NewTieredActionPlanReservationCoordinator(ITieredActionPlanReservationStore store, IClock clock, Registry flags)
    {
        return new TieredActionPlanReservationCoordinator(
            store,
            TieredActionPlanReservationRules.DefaultTieredActionPlanReservationValidator(),
            clock,
            new List<string>(),
            TieredActionPlanReservationEngine.DefaultTieredActionPlanReservationEngine(flags));
    }
}
