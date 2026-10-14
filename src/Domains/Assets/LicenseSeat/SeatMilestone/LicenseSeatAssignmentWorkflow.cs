namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatAssignmentDesk(ILicenseSeatAssignmentStore Store, LicenseSeatAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatAssignmentStatus.Stopped);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of license seat assignment workflow.</summary>
public static class LicenseSeatAssignmentWorkflow
{
    public static LicenseSeatAssignmentDesk NewLicenseSeatAssignmentDesk(ILicenseSeatAssignmentStore store, IClock clock)
    {
        return new LicenseSeatAssignmentDesk(
            store,
            LicenseSeatAssignmentRules.DefaultLicenseSeatAssignmentValidator(),
            clock,
            new List<string>());
    }
}
