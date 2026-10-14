namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves license seat metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatMetricWorkflow(ILicenseSeatMetricStore Store, LicenseSeatMetricValidator Validator, IClock Clock, List<string> Journal, LicenseSeatMetricEngine Engine)
{
    public static LicenseSeatMetricWorkflow NewLicenseSeatMetricWorkflow(ILicenseSeatMetricStore store, IClock clock, Registry flags)
    {
        return new LicenseSeatMetricWorkflow(
            store,
            LicenseSeatMetricRules.DefaultLicenseSeatMetricValidator(),
            clock,
            new List<string>(),
            LicenseSeatMetricEngine.DefaultLicenseSeatMetricEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat metric {code} not found");
        }
        var result = Engine.Evaluate(item);
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
