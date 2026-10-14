namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient deduction tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientDeductionTierManager(ILenientDeductionTierStore Store, LenientDeductionTierValidator Validator, IClock Clock, List<string> Journal, LenientDeductionTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientDeductionTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientDeductionTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction tier {code} not found");
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

/// <summary>Functions of lenient deduction tier workflow.</summary>
public static class LenientDeductionTierWorkflow
{
    public static LenientDeductionTierManager NewLenientDeductionTierManager(ILenientDeductionTierStore store, IClock clock)
    {
        return new LenientDeductionTierManager(
            store,
            LenientDeductionTierRules.DefaultLenientDeductionTierValidator(),
            clock,
            new List<string>(),
            LenientDeductionTierEngine.DefaultLenientDeductionTierEngine());
    }
}
