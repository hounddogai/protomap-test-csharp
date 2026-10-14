namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves license seat baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatBaselineManager(ILicenseSeatBaselineStore Store, LicenseSeatBaselineValidator Validator, IClock Clock, List<string> Journal, LicenseSeatBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatBaselineStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatBaselineStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat baseline {code} not found");
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

/// <summary>Functions of license seat baseline workflow.</summary>
public static class LicenseSeatBaselineWorkflow
{
    public static LicenseSeatBaselineManager NewLicenseSeatBaselineManager(ILicenseSeatBaselineStore store, IClock clock)
    {
        return new LicenseSeatBaselineManager(
            store,
            LicenseSeatBaselineRules.DefaultLicenseSeatBaselineValidator(),
            clock,
            new List<string>(),
            LicenseSeatBaselineEngine.DefaultLicenseSeatBaselineEngine());
    }
}
