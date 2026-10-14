namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandPlanWorkflow(IPayBandPlanStore Store, PayBandPlanValidator Validator, IClock Clock, List<string> Journal)
{
    public static PayBandPlanWorkflow NewPayBandPlanWorkflow(IPayBandPlanStore store, IClock clock)
    {
        return new PayBandPlanWorkflow(
            store,
            PayBandPlanRules.DefaultPayBandPlanValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
