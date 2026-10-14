namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves return plan manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanManifestWorkflow(IReturnPlanManifestStore Store, ReturnPlanManifestValidator Validator, IClock Clock, List<string> Journal)
{
    public static ReturnPlanManifestWorkflow NewReturnPlanManifestWorkflow(IReturnPlanManifestStore store, IClock clock)
    {
        return new ReturnPlanManifestWorkflow(
            store,
            ReturnPlanManifestRules.DefaultReturnPlanManifestValidator(),
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
            throw new InvalidOperationException($"return plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanManifestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanManifestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Sweep(int cutoff)
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
