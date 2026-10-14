namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipReservationCoordinator(IPayslipReservationStore Store, PayslipReservationValidator Validator, IClock Clock, List<string> Journal, PayslipReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip reservation {code} not found");
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

/// <summary>Functions of payslip reservation workflow.</summary>
public static class PayslipReservationWorkflow
{
    public static PayslipReservationCoordinator NewPayslipReservationCoordinator(IPayslipReservationStore store, IClock clock)
    {
        return new PayslipReservationCoordinator(
            store,
            PayslipReservationRules.DefaultPayslipReservationValidator(),
            clock,
            new List<string>(),
            PayslipReservationEngine.DefaultPayslipReservationEngine());
    }
}
