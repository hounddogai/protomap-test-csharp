namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual nomination scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualNominationScoreWorkflow(IManualNominationScoreStore Store, ManualNominationScoreValidator Validator, IClock Clock, List<string> Journal, ManualNominationScoreEngine Engine)
{
    public static ManualNominationScoreWorkflow NewManualNominationScoreWorkflow(IManualNominationScoreStore store, IClock clock, Registry flags)
    {
        return new ManualNominationScoreWorkflow(
            store,
            ManualNominationScoreRules.DefaultManualNominationScoreValidator(),
            clock,
            new List<string>(),
            ManualNominationScoreEngine.DefaultManualNominationScoreEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual nomination score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualNominationScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual nomination score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualNominationScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual nomination score {code} not found");
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
