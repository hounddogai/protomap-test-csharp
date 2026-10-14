namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountBaselineWorkflow(IHeadcountBaselineStore Store, HeadcountBaselineValidator Validator, IClock Clock, List<string> Journal, HeadcountBaselineEngine Engine)
{
    public static HeadcountBaselineWorkflow NewHeadcountBaselineWorkflow(IHeadcountBaselineStore store, IClock clock, Registry flags)
    {
        return new HeadcountBaselineWorkflow(
            store,
            HeadcountBaselineRules.DefaultHeadcountBaselineValidator(),
            clock,
            new List<string>(),
            HeadcountBaselineEngine.DefaultHeadcountBaselineEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount baseline {code} not found");
        }
        var result = Engine.Run(item);
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
