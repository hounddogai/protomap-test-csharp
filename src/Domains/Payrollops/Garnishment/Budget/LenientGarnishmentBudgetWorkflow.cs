namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient garnishment budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientGarnishmentBudgetDesk(ILenientGarnishmentBudgetStore Store, LenientGarnishmentBudgetValidator Validator, IClock Clock, List<string> Journal, LenientGarnishmentBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient garnishment budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientGarnishmentBudgetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient garnishment budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientGarnishmentBudgetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient garnishment budget {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of lenient garnishment budget workflow.</summary>
public static class LenientGarnishmentBudgetWorkflow
{
    public static LenientGarnishmentBudgetDesk NewLenientGarnishmentBudgetDesk(ILenientGarnishmentBudgetStore store, IClock clock, Registry flags)
    {
        return new LenientGarnishmentBudgetDesk(
            store,
            LenientGarnishmentBudgetRules.DefaultLenientGarnishmentBudgetValidator(),
            clock,
            new List<string>(),
            LenientGarnishmentBudgetEngine.DefaultLenientGarnishmentBudgetEngine(flags));
    }
}
