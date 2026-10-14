namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped tax form assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedTaxFormAssignmentManager(ICappedTaxFormAssignmentStore Store, CappedTaxFormAssignmentValidator Validator, IClock Clock, List<string> Journal, CappedTaxFormAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedTaxFormAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedTaxFormAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped tax form assignment {code} not found");
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

/// <summary>Functions of capped tax form assignment workflow.</summary>
public static class CappedTaxFormAssignmentWorkflow
{
    public static CappedTaxFormAssignmentManager NewCappedTaxFormAssignmentManager(ICappedTaxFormAssignmentStore store, IClock clock, Registry flags)
    {
        return new CappedTaxFormAssignmentManager(
            store,
            CappedTaxFormAssignmentRules.DefaultCappedTaxFormAssignmentValidator(),
            clock,
            new List<string>(),
            CappedTaxFormAssignmentEngine.DefaultCappedTaxFormAssignmentEngine(flags));
    }
}
