namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual booking audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualBookingAuditManager(IManualBookingAuditStore Store, ManualBookingAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual booking audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBookingAuditStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual booking audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBookingAuditStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of manual booking audit workflow.</summary>
public static class ManualBookingAuditWorkflow
{
    public static ManualBookingAuditManager NewManualBookingAuditManager(IManualBookingAuditStore store, IClock clock)
    {
        return new ManualBookingAuditManager(
            store,
            ManualBookingAuditRules.DefaultManualBookingAuditValidator(),
            clock,
            new List<string>());
    }
}
