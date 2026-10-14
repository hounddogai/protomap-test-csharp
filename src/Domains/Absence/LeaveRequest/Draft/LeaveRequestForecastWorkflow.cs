namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestForecastWorkflow(ILeaveRequestForecastStore Store, LeaveRequestForecastValidator Validator, IClock Clock, List<string> Journal, LeaveRequestForecastEngine Engine)
{
    public static LeaveRequestForecastWorkflow NewLeaveRequestForecastWorkflow(ILeaveRequestForecastStore store, IClock clock)
    {
        return new LeaveRequestForecastWorkflow(
            store,
            LeaveRequestForecastRules.DefaultLeaveRequestForecastValidator(),
            clock,
            new List<string>(),
            LeaveRequestForecastEngine.DefaultLeaveRequestForecastEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestForecastStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestForecastStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request forecast {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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
