namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal garnishment notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalGarnishmentNoticeCoordinator(ISeasonalGarnishmentNoticeStore Store, SeasonalGarnishmentNoticeValidator Validator, IClock Clock, List<string> Journal, SeasonalGarnishmentNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal garnishment notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalGarnishmentNoticeStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal garnishment notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalGarnishmentNoticeStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal garnishment notice {code} not found");
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

/// <summary>Functions of seasonal garnishment notice workflow.</summary>
public static class SeasonalGarnishmentNoticeWorkflow
{
    public static SeasonalGarnishmentNoticeCoordinator NewSeasonalGarnishmentNoticeCoordinator(ISeasonalGarnishmentNoticeStore store, IClock clock)
    {
        return new SeasonalGarnishmentNoticeCoordinator(
            store,
            SeasonalGarnishmentNoticeRules.DefaultSeasonalGarnishmentNoticeValidator(),
            clock,
            new List<string>(),
            SeasonalGarnishmentNoticeEngine.DefaultSeasonalGarnishmentNoticeEngine());
    }
}
