namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling succession plan requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingSuccessionPlanRequestWorkflow(IRollingSuccessionPlanRequestStore Store, RollingSuccessionPlanRequestValidator Validator, IClock Clock, List<string> Journal)
{
    public static RollingSuccessionPlanRequestWorkflow NewRollingSuccessionPlanRequestWorkflow(IRollingSuccessionPlanRequestStore store, IClock clock)
    {
        return new RollingSuccessionPlanRequestWorkflow(
            store,
            RollingSuccessionPlanRequestRules.DefaultRollingSuccessionPlanRequestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingSuccessionPlanRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling succession plan request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingSuccessionPlanRequestStatus.Withdrawn);
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
