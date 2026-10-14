namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves garnishment schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentScheduleWorkflow(IGarnishmentScheduleStore Store, GarnishmentScheduleValidator Validator, IClock Clock, List<string> Journal, GarnishmentScheduleEngine Engine)
{
    public static GarnishmentScheduleWorkflow NewGarnishmentScheduleWorkflow(IGarnishmentScheduleStore store, IClock clock, Registry flags)
    {
        return new GarnishmentScheduleWorkflow(
            store,
            GarnishmentScheduleRules.DefaultGarnishmentScheduleValidator(),
            clock,
            new List<string>(),
            GarnishmentScheduleEngine.DefaultGarnishmentScheduleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment schedule {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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
