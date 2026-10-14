namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualMetricWorkflow(IAccrualMetricStore Store, AccrualMetricValidator Validator, IClock Clock, List<string> Journal, AccrualMetricEngine Engine)
{
    public static AccrualMetricWorkflow NewAccrualMetricWorkflow(IAccrualMetricStore store, IClock clock)
    {
        return new AccrualMetricWorkflow(
            store,
            AccrualMetricRules.DefaultAccrualMetricValidator(),
            clock,
            new List<string>(),
            AccrualMetricEngine.DefaultAccrualMetricEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual metric {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
