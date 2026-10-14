namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict succession plan scores through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictSuccessionPlanScoreWorkflow(IStrictSuccessionPlanScoreStore Store, StrictSuccessionPlanScoreValidator Validator, IClock Clock, List<string> Journal, StrictSuccessionPlanScoreEngine Engine)
{
    public static StrictSuccessionPlanScoreWorkflow NewStrictSuccessionPlanScoreWorkflow(IStrictSuccessionPlanScoreStore store, IClock clock, Registry flags)
    {
        return new StrictSuccessionPlanScoreWorkflow(
            store,
            StrictSuccessionPlanScoreRules.DefaultStrictSuccessionPlanScoreValidator(),
            clock,
            new List<string>(),
            StrictSuccessionPlanScoreEngine.DefaultStrictSuccessionPlanScoreEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict succession plan score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSuccessionPlanScoreStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict succession plan score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSuccessionPlanScoreStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict succession plan score {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
