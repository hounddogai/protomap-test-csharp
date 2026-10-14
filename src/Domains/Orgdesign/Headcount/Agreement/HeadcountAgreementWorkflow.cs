namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountAgreementDesk(IHeadcountAgreementStore Store, HeadcountAgreementValidator Validator, IClock Clock, List<string> Journal, HeadcountAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountAgreementStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountAgreementStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount agreement {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of headcount agreement workflow.</summary>
public static class HeadcountAgreementWorkflow
{
    public static HeadcountAgreementDesk NewHeadcountAgreementDesk(IHeadcountAgreementStore store, IClock clock, Registry flags)
    {
        return new HeadcountAgreementDesk(
            store,
            HeadcountAgreementRules.DefaultHeadcountAgreementValidator(),
            clock,
            new List<string>(),
            HeadcountAgreementEngine.DefaultHeadcountAgreementEngine(flags));
    }
}
