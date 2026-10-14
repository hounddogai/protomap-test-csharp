namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive tax form records through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveTaxFormRecordDesk(IAdaptiveTaxFormRecordStore Store, AdaptiveTaxFormRecordValidator Validator, IClock Clock, List<string> Journal, AdaptiveTaxFormRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive tax form record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTaxFormRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive tax form record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTaxFormRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive tax form record {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of adaptive tax form record workflow.</summary>
public static class AdaptiveTaxFormRecordWorkflow
{
    public static AdaptiveTaxFormRecordDesk NewAdaptiveTaxFormRecordDesk(IAdaptiveTaxFormRecordStore store, IClock clock, Registry flags)
    {
        return new AdaptiveTaxFormRecordDesk(
            store,
            AdaptiveTaxFormRecordRules.DefaultAdaptiveTaxFormRecordValidator(),
            clock,
            new List<string>(),
            AdaptiveTaxFormRecordEngine.DefaultAdaptiveTaxFormRecordEngine(flags));
    }
}
