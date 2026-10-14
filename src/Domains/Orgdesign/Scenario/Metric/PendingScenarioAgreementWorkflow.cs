namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending scenario agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingScenarioAgreementCoordinator(IPendingScenarioAgreementStore Store, PendingScenarioAgreementValidator Validator, IClock Clock, List<string> Journal, PendingScenarioAgreementEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingScenarioAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingScenarioAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending scenario agreement {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of pending scenario agreement workflow.</summary>
public static class PendingScenarioAgreementWorkflow
{
    public static PendingScenarioAgreementCoordinator NewPendingScenarioAgreementCoordinator(IPendingScenarioAgreementStore store, IClock clock, Registry flags)
    {
        return new PendingScenarioAgreementCoordinator(
            store,
            PendingScenarioAgreementRules.DefaultPendingScenarioAgreementValidator(),
            clock,
            new List<string>(),
            PendingScenarioAgreementEngine.DefaultPendingScenarioAgreementEngine(flags));
    }
}
