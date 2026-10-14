namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling retro pay summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingRetroPaySummaryWorkflow(IRollingRetroPaySummaryStore Store, RollingRetroPaySummaryValidator Validator, IClock Clock, List<string> Journal)
{
    public static RollingRetroPaySummaryWorkflow NewRollingRetroPaySummaryWorkflow(IRollingRetroPaySummaryStore store, IClock clock)
    {
        return new RollingRetroPaySummaryWorkflow(
            store,
            RollingRetroPaySummaryRules.DefaultRollingRetroPaySummaryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling retro pay summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRetroPaySummaryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling retro pay summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRetroPaySummaryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
