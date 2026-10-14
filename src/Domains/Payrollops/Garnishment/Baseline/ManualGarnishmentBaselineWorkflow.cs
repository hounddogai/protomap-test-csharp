namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual garnishment baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualGarnishmentBaselineService(IManualGarnishmentBaselineStore Store, ManualGarnishmentBaselineValidator Validator, IClock Clock, List<string> Journal, ManualGarnishmentBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual garnishment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualGarnishmentBaselineStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual garnishment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualGarnishmentBaselineStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual garnishment baseline {code} not found");
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

/// <summary>Functions of manual garnishment baseline workflow.</summary>
public static class ManualGarnishmentBaselineWorkflow
{
    public static ManualGarnishmentBaselineService NewManualGarnishmentBaselineService(IManualGarnishmentBaselineStore store, IClock clock, Registry flags)
    {
        return new ManualGarnishmentBaselineService(
            store,
            ManualGarnishmentBaselineRules.DefaultManualGarnishmentBaselineValidator(),
            clock,
            new List<string>(),
            ManualGarnishmentBaselineEngine.DefaultManualGarnishmentBaselineEngine(flags));
    }
}
