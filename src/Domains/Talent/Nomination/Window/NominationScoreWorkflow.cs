namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationScoreWorkflow(INominationScoreStore Store, NominationScoreValidator Validator, IClock Clock, List<string> Journal, NominationScoreEngine Engine)
{
    public static NominationScoreWorkflow NewNominationScoreWorkflow(INominationScoreStore store, IClock clock)
    {
        return new NominationScoreWorkflow(
            store,
            NominationScoreRules.DefaultNominationScoreValidator(),
            clock,
            new List<string>(),
            NominationScoreEngine.DefaultNominationScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination score {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.EndsAt > cutoff)
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
