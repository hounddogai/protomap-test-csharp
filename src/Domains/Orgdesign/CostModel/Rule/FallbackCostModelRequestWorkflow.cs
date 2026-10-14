namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback cost model requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackCostModelRequestDesk(IFallbackCostModelRequestStore Store, FallbackCostModelRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback cost model request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackCostModelRequestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback cost model request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackCostModelRequestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of fallback cost model request workflow.</summary>
public static class FallbackCostModelRequestWorkflow
{
    public static FallbackCostModelRequestDesk NewFallbackCostModelRequestDesk(IFallbackCostModelRequestStore store, IClock clock)
    {
        return new FallbackCostModelRequestDesk(
            store,
            FallbackCostModelRequestRules.DefaultFallbackCostModelRequestValidator(),
            clock,
            new List<string>());
    }
}
