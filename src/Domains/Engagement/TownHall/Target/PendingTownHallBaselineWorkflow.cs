namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending town hall baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingTownHallBaselineWorkflow(IPendingTownHallBaselineStore Store, PendingTownHallBaselineValidator Validator, IClock Clock, List<string> Journal, PendingTownHallBaselineEngine Engine)
{
    public static PendingTownHallBaselineWorkflow NewPendingTownHallBaselineWorkflow(IPendingTownHallBaselineStore store, IClock clock)
    {
        return new PendingTownHallBaselineWorkflow(
            store,
            PendingTownHallBaselineRules.DefaultPendingTownHallBaselineValidator(),
            clock,
            new List<string>(),
            PendingTownHallBaselineEngine.DefaultPendingTownHallBaselineEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTownHallBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTownHallBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending town hall baseline {code} not found");
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
