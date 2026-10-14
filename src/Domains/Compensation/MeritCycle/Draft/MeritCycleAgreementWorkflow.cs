namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleAgreementWorkflow(IMeritCycleAgreementStore Store, MeritCycleAgreementValidator Validator, IClock Clock, List<string> Journal, MeritCycleAgreementEngine Engine)
{
    public static MeritCycleAgreementWorkflow NewMeritCycleAgreementWorkflow(IMeritCycleAgreementStore store, IClock clock, Registry flags)
    {
        return new MeritCycleAgreementWorkflow(
            store,
            MeritCycleAgreementRules.DefaultMeritCycleAgreementValidator(),
            clock,
            new List<string>(),
            MeritCycleAgreementEngine.DefaultMeritCycleAgreementEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle agreement {code} not found");
        }
        var result = Engine.Evaluate(item);
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
