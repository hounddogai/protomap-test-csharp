namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SuccessionPlanReservationService(ISuccessionPlanReservationStore Store, SuccessionPlanReservationValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanReservationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanReservationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan reservation {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of succession plan reservation workflow.</summary>
public static class SuccessionPlanReservationWorkflow
{
    public static SuccessionPlanReservationService NewSuccessionPlanReservationService(ISuccessionPlanReservationStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanReservationService(
            store,
            SuccessionPlanReservationRules.DefaultSuccessionPlanReservationValidator(),
            clock,
            new List<string>(),
            SuccessionPlanReservationEngine.DefaultSuccessionPlanReservationEngine(flags));
    }
}
