namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional pay run variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalPayRunVarianceWorkflow(IRegionalPayRunVarianceStore Store, RegionalPayRunVarianceValidator Validator, IClock Clock, List<string> Journal, RegionalPayRunVarianceEngine Engine)
{
    public static RegionalPayRunVarianceWorkflow NewRegionalPayRunVarianceWorkflow(IRegionalPayRunVarianceStore store, IClock clock)
    {
        return new RegionalPayRunVarianceWorkflow(
            store,
            RegionalPayRunVarianceRules.DefaultRegionalPayRunVarianceValidator(),
            clock,
            new List<string>(),
            RegionalPayRunVarianceEngine.DefaultRegionalPayRunVarianceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPayRunVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPayRunVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pay run variance {code} not found");
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
