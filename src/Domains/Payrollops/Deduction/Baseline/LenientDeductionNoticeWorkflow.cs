namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient deduction notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientDeductionNoticeWorkflow(ILenientDeductionNoticeStore Store, LenientDeductionNoticeValidator Validator, IClock Clock, List<string> Journal, LenientDeductionNoticeEngine Engine)
{
    public static LenientDeductionNoticeWorkflow NewLenientDeductionNoticeWorkflow(ILenientDeductionNoticeStore store, IClock clock, Registry flags)
    {
        return new LenientDeductionNoticeWorkflow(
            store,
            LenientDeductionNoticeRules.DefaultLenientDeductionNoticeValidator(),
            clock,
            new List<string>(),
            LenientDeductionNoticeEngine.DefaultLenientDeductionNoticeEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientDeductionNoticeStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientDeductionNoticeStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient deduction notice {code} not found");
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
