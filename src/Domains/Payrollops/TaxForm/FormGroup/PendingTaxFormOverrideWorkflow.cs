namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending tax form overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingTaxFormOverrideService(IPendingTaxFormOverrideStore Store, PendingTaxFormOverrideValidator Validator, IClock Clock, List<string> Journal, PendingTaxFormOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending tax form override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTaxFormOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending tax form override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTaxFormOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending tax form override {code} not found");
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

/// <summary>Functions of pending tax form override workflow.</summary>
public static class PendingTaxFormOverrideWorkflow
{
    public static PendingTaxFormOverrideService NewPendingTaxFormOverrideService(IPendingTaxFormOverrideStore store, IClock clock, Registry flags)
    {
        return new PendingTaxFormOverrideService(
            store,
            PendingTaxFormOverrideRules.DefaultPendingTaxFormOverrideValidator(),
            clock,
            new List<string>(),
            PendingTaxFormOverrideEngine.DefaultPendingTaxFormOverrideEngine(flags));
    }
}
