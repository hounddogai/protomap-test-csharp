namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanProposalWorkflow(ISuccessionPlanProposalStore Store, SuccessionPlanProposalValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanProposalEngine Engine)
{
    public static SuccessionPlanProposalWorkflow NewSuccessionPlanProposalWorkflow(ISuccessionPlanProposalStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanProposalWorkflow(
            store,
            SuccessionPlanProposalRules.DefaultSuccessionPlanProposalValidator(),
            clock,
            new List<string>(),
            SuccessionPlanProposalEngine.DefaultSuccessionPlanProposalEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanProposalStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanProposalStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan proposal {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.StartsAt > cutoff)
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
