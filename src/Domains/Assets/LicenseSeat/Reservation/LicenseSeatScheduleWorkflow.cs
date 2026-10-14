namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatScheduleDesk(ILicenseSeatScheduleStore Store, LicenseSeatScheduleValidator Validator, IClock Clock, List<string> Journal, LicenseSeatScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat schedule {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of license seat schedule workflow.</summary>
public static class LicenseSeatScheduleWorkflow
{
    public static LicenseSeatScheduleDesk NewLicenseSeatScheduleDesk(ILicenseSeatScheduleStore store, IClock clock)
    {
        return new LicenseSeatScheduleDesk(
            store,
            LicenseSeatScheduleRules.DefaultLicenseSeatScheduleValidator(),
            clock,
            new List<string>(),
            LicenseSeatScheduleEngine.DefaultLicenseSeatScheduleEngine());
    }
}
