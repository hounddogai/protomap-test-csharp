namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationAdjustmentWorkflow(INominationAdjustmentStore Store, NominationAdjustmentValidator Validator, IClock Clock, List<string> Journal, NominationAdjustmentEngine Engine)
{
    public static NominationAdjustmentWorkflow NewNominationAdjustmentWorkflow(INominationAdjustmentStore store, IClock clock)
    {
        return new NominationAdjustmentWorkflow(
            store,
            NominationAdjustmentRules.DefaultNominationAdjustmentValidator(),
            clock,
            new List<string>(),
            NominationAdjustmentEngine.DefaultNominationAdjustmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination adjustment {code} not found");
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
