namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional assignment agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAssignmentAgreementService(IRegionalAssignmentAgreementStore Store, RegionalAssignmentAgreementValidator Validator, IClock Clock, List<string> Journal, RegionalAssignmentAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional assignment agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAssignmentAgreementStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional assignment agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAssignmentAgreementStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional assignment agreement {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of regional assignment agreement workflow.</summary>
public static class RegionalAssignmentAgreementWorkflow
{
    public static RegionalAssignmentAgreementService NewRegionalAssignmentAgreementService(IRegionalAssignmentAgreementStore store, IClock clock, Registry flags)
    {
        return new RegionalAssignmentAgreementService(
            store,
            RegionalAssignmentAgreementRules.DefaultRegionalAssignmentAgreementValidator(),
            clock,
            new List<string>(),
            RegionalAssignmentAgreementEngine.DefaultRegionalAssignmentAgreementEngine(flags));
    }
}
