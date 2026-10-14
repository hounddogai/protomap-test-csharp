namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary deduction budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryDeductionBudgetWorkflow(IPrimaryDeductionBudgetStore Store, PrimaryDeductionBudgetValidator Validator, IClock Clock, List<string> Journal, PrimaryDeductionBudgetEngine Engine)
{
    public static PrimaryDeductionBudgetWorkflow NewPrimaryDeductionBudgetWorkflow(IPrimaryDeductionBudgetStore store, IClock clock, Registry flags)
    {
        return new PrimaryDeductionBudgetWorkflow(
            store,
            PrimaryDeductionBudgetRules.DefaultPrimaryDeductionBudgetValidator(),
            clock,
            new List<string>(),
            PrimaryDeductionBudgetEngine.DefaultPrimaryDeductionBudgetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary deduction budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryDeductionBudgetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary deduction budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryDeductionBudgetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary deduction budget {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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
