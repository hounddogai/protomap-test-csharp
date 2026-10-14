namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleReviewWorkflow(IMeritCycleReviewStore Store, MeritCycleReviewValidator Validator, IClock Clock, List<string> Journal, MeritCycleReviewEngine Engine)
{
    public static MeritCycleReviewWorkflow NewMeritCycleReviewWorkflow(IMeritCycleReviewStore store, IClock clock, Registry flags)
    {
        return new MeritCycleReviewWorkflow(
            store,
            MeritCycleReviewRules.DefaultMeritCycleReviewValidator(),
            clock,
            new List<string>(),
            MeritCycleReviewEngine.DefaultMeritCycleReviewEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleReviewStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleReviewStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle review {code} not found");
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
            if (item.UpdatedAt > cutoff)
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
