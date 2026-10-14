namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves offboarding assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingAssignmentWorkflow(IOffboardingAssignmentStore Store, OffboardingAssignmentValidator Validator, IClock Clock, List<string> Journal, OffboardingAssignmentEngine Engine)
{
    public static OffboardingAssignmentWorkflow NewOffboardingAssignmentWorkflow(IOffboardingAssignmentStore store, IClock clock, Registry flags)
    {
        return new OffboardingAssignmentWorkflow(
            store,
            OffboardingAssignmentRules.DefaultOffboardingAssignmentValidator(),
            clock,
            new List<string>(),
            OffboardingAssignmentEngine.DefaultOffboardingAssignmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding assignment {code} not found");
        }
        var result = Engine.Run(item);
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
