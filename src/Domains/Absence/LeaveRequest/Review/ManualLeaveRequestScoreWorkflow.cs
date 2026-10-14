namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual leave request scores through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualLeaveRequestScoreDesk(IManualLeaveRequestScoreStore Store, ManualLeaveRequestScoreValidator Validator, IClock Clock, List<string> Journal, ManualLeaveRequestScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualLeaveRequestScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualLeaveRequestScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request score {code} not found");
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

/// <summary>Functions of manual leave request score workflow.</summary>
public static class ManualLeaveRequestScoreWorkflow
{
    public static ManualLeaveRequestScoreDesk NewManualLeaveRequestScoreDesk(IManualLeaveRequestScoreStore store, IClock clock, Registry flags)
    {
        return new ManualLeaveRequestScoreDesk(
            store,
            ManualLeaveRequestScoreRules.DefaultManualLeaveRequestScoreValidator(),
            clock,
            new List<string>(),
            ManualLeaveRequestScoreEngine.DefaultManualLeaveRequestScoreEngine(flags));
    }
}
