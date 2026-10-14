namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global license seat assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalLicenseSeatAssignmentDesk(IGlobalLicenseSeatAssignmentStore Store, GlobalLicenseSeatAssignmentValidator Validator, IClock Clock, List<string> Journal, GlobalLicenseSeatAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalLicenseSeatAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalLicenseSeatAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global license seat assignment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global license seat assignment workflow.</summary>
public static class GlobalLicenseSeatAssignmentWorkflow
{
    public static GlobalLicenseSeatAssignmentDesk NewGlobalLicenseSeatAssignmentDesk(IGlobalLicenseSeatAssignmentStore store, IClock clock, Registry flags)
    {
        return new GlobalLicenseSeatAssignmentDesk(
            store,
            GlobalLicenseSeatAssignmentRules.DefaultGlobalLicenseSeatAssignmentValidator(),
            clock,
            new List<string>(),
            GlobalLicenseSeatAssignmentEngine.DefaultGlobalLicenseSeatAssignmentEngine(flags));
    }
}
