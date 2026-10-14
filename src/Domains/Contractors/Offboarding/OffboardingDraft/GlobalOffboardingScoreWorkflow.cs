namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global offboarding scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalOffboardingScoreDesk(IGlobalOffboardingScoreStore Store, GlobalOffboardingScoreValidator Validator, IClock Clock, List<string> Journal, GlobalOffboardingScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global offboarding score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalOffboardingScoreStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global offboarding score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalOffboardingScoreStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global offboarding score {code} not found");
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

/// <summary>Functions of global offboarding score workflow.</summary>
public static class GlobalOffboardingScoreWorkflow
{
    public static GlobalOffboardingScoreDesk NewGlobalOffboardingScoreDesk(IGlobalOffboardingScoreStore store, IClock clock, Registry flags)
    {
        return new GlobalOffboardingScoreDesk(
            store,
            GlobalOffboardingScoreRules.DefaultGlobalOffboardingScoreValidator(),
            clock,
            new List<string>(),
            GlobalOffboardingScoreEngine.DefaultGlobalOffboardingScoreEngine(flags));
    }
}
