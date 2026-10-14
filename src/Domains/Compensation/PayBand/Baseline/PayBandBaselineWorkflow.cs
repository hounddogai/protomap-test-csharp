namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandBaselineDesk(IPayBandBaselineStore Store, PayBandBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandBaselineStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pay band baseline workflow.</summary>
public static class PayBandBaselineWorkflow
{
    public static PayBandBaselineDesk NewPayBandBaselineDesk(IPayBandBaselineStore store, IClock clock)
    {
        return new PayBandBaselineDesk(
            store,
            PayBandBaselineRules.DefaultPayBandBaselineValidator(),
            clock,
            new List<string>());
    }
}
