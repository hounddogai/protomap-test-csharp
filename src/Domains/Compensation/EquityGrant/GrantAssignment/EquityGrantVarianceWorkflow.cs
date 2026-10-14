namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantVarianceDesk(IEquityGrantVarianceStore Store, EquityGrantVarianceValidator Validator, IClock Clock, List<string> Journal, EquityGrantVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantVarianceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantVarianceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant variance {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of equity grant variance workflow.</summary>
public static class EquityGrantVarianceWorkflow
{
    public static EquityGrantVarianceDesk NewEquityGrantVarianceDesk(IEquityGrantVarianceStore store, IClock clock)
    {
        return new EquityGrantVarianceDesk(
            store,
            EquityGrantVarianceRules.DefaultEquityGrantVarianceValidator(),
            clock,
            new List<string>(),
            EquityGrantVarianceEngine.DefaultEquityGrantVarianceEngine());
    }
}
