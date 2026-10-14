namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled tax form templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledTaxFormTemplateDesk(IScheduledTaxFormTemplateStore Store, ScheduledTaxFormTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled tax form template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledTaxFormTemplateStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled tax form template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledTaxFormTemplateStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of scheduled tax form template workflow.</summary>
public static class ScheduledTaxFormTemplateWorkflow
{
    public static ScheduledTaxFormTemplateDesk NewScheduledTaxFormTemplateDesk(IScheduledTaxFormTemplateStore store, IClock clock)
    {
        return new ScheduledTaxFormTemplateDesk(
            store,
            ScheduledTaxFormTemplateRules.DefaultScheduledTaxFormTemplateValidator(),
            clock,
            new List<string>());
    }
}
