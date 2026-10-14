namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global deduction reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalDeductionReservationWorkflow(IGlobalDeductionReservationStore Store, GlobalDeductionReservationValidator Validator, IClock Clock, List<string> Journal, GlobalDeductionReservationEngine Engine)
{
    public static GlobalDeductionReservationWorkflow NewGlobalDeductionReservationWorkflow(IGlobalDeductionReservationStore store, IClock clock, Registry flags)
    {
        return new GlobalDeductionReservationWorkflow(
            store,
            GlobalDeductionReservationRules.DefaultGlobalDeductionReservationValidator(),
            clock,
            new List<string>(),
            GlobalDeductionReservationEngine.DefaultGlobalDeductionReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global deduction reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalDeductionReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global deduction reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalDeductionReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global deduction reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
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
