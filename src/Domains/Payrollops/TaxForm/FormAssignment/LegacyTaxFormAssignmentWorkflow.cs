namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy tax form assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyTaxFormAssignmentManager(ILegacyTaxFormAssignmentStore Store, LegacyTaxFormAssignmentValidator Validator, IClock Clock, List<string> Journal, LegacyTaxFormAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy tax form assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTaxFormAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy tax form assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTaxFormAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy tax form assignment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of legacy tax form assignment workflow.</summary>
public static class LegacyTaxFormAssignmentWorkflow
{
    public static LegacyTaxFormAssignmentManager NewLegacyTaxFormAssignmentManager(ILegacyTaxFormAssignmentStore store, IClock clock, Registry flags)
    {
        return new LegacyTaxFormAssignmentManager(
            store,
            LegacyTaxFormAssignmentRules.DefaultLegacyTaxFormAssignmentValidator(),
            clock,
            new List<string>(),
            LegacyTaxFormAssignmentEngine.DefaultLegacyTaxFormAssignmentEngine(flags));
    }
}
