namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves retro pay summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPaySummaryWorkflow(IRetroPaySummaryStore Store, RetroPaySummaryValidator Validator, IClock Clock, List<string> Journal, RetroPaySummaryEngine Engine)
{
    public static RetroPaySummaryWorkflow NewRetroPaySummaryWorkflow(IRetroPaySummaryStore store, IClock clock, Registry flags)
    {
        return new RetroPaySummaryWorkflow(
            store,
            RetroPaySummaryRules.DefaultRetroPaySummaryValidator(),
            clock,
            new List<string>(),
            RetroPaySummaryEngine.DefaultRetroPaySummaryEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPaySummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPaySummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay summary {code} not found");
        }
        var result = Engine.Compute(item);
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
