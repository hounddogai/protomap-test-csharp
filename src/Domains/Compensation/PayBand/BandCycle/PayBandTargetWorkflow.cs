namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandTargetWorkflow(IPayBandTargetStore Store, PayBandTargetValidator Validator, IClock Clock, List<string> Journal, PayBandTargetEngine Engine)
{
    public static PayBandTargetWorkflow NewPayBandTargetWorkflow(IPayBandTargetStore store, IClock clock)
    {
        return new PayBandTargetWorkflow(
            store,
            PayBandTargetRules.DefaultPayBandTargetValidator(),
            clock,
            new List<string>(),
            PayBandTargetEngine.DefaultPayBandTargetEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band target {code} not found");
        }
        var result = Engine.Run(item);
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
