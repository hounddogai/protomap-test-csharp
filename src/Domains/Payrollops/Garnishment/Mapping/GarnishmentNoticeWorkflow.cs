namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentNoticeCoordinator(IGarnishmentNoticeStore Store, GarnishmentNoticeValidator Validator, IClock Clock, List<string> Journal, GarnishmentNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentNoticeStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentNoticeStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment notice {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of garnishment notice workflow.</summary>
public static class GarnishmentNoticeWorkflow
{
    public static GarnishmentNoticeCoordinator NewGarnishmentNoticeCoordinator(IGarnishmentNoticeStore store, IClock clock)
    {
        return new GarnishmentNoticeCoordinator(
            store,
            GarnishmentNoticeRules.DefaultGarnishmentNoticeValidator(),
            clock,
            new List<string>(),
            GarnishmentNoticeEngine.DefaultGarnishmentNoticeEngine());
    }
}
