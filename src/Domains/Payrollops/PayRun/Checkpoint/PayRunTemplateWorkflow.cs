namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay run templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayRunTemplateWorkflow(IPayRunTemplateStore Store, PayRunTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    public static PayRunTemplateWorkflow NewPayRunTemplateWorkflow(IPayRunTemplateStore store, IClock clock)
    {
        return new PayRunTemplateWorkflow(
            store,
            PayRunTemplateRules.DefaultPayRunTemplateValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayRunTemplateStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay run template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayRunTemplateStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
