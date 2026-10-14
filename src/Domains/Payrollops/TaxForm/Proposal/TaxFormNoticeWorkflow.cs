namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormNoticeCoordinator(ITaxFormNoticeStore Store, TaxFormNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormNoticeStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormNoticeStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of tax form notice workflow.</summary>
public static class TaxFormNoticeWorkflow
{
    public static TaxFormNoticeCoordinator NewTaxFormNoticeCoordinator(ITaxFormNoticeStore store, IClock clock)
    {
        return new TaxFormNoticeCoordinator(
            store,
            TaxFormNoticeRules.DefaultTaxFormNoticeValidator(),
            clock,
            new List<string>());
    }
}
