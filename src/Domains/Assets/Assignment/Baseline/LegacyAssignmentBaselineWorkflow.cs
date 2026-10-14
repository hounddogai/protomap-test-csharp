namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy assignment baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyAssignmentBaselineService(ILegacyAssignmentBaselineStore Store, LegacyAssignmentBaselineValidator Validator, IClock Clock, List<string> Journal, LegacyAssignmentBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy assignment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyAssignmentBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy assignment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyAssignmentBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy assignment baseline {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of legacy assignment baseline workflow.</summary>
public static class LegacyAssignmentBaselineWorkflow
{
    public static LegacyAssignmentBaselineService NewLegacyAssignmentBaselineService(ILegacyAssignmentBaselineStore store, IClock clock, Registry flags)
    {
        return new LegacyAssignmentBaselineService(
            store,
            LegacyAssignmentBaselineRules.DefaultLegacyAssignmentBaselineValidator(),
            clock,
            new List<string>(),
            LegacyAssignmentBaselineEngine.DefaultLegacyAssignmentBaselineEngine(flags));
    }
}
