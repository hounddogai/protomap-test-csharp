namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered pay band schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPayBandScheduleWorkflow(ITieredPayBandScheduleStore Store, TieredPayBandScheduleValidator Validator, IClock Clock, List<string> Journal, TieredPayBandScheduleEngine Engine)
{
    public static TieredPayBandScheduleWorkflow NewTieredPayBandScheduleWorkflow(ITieredPayBandScheduleStore store, IClock clock)
    {
        return new TieredPayBandScheduleWorkflow(
            store,
            TieredPayBandScheduleRules.DefaultTieredPayBandScheduleValidator(),
            clock,
            new List<string>(),
            TieredPayBandScheduleEngine.DefaultTieredPayBandScheduleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pay band schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPayBandScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pay band schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPayBandScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pay band schedule {code} not found");
        }
        var result = Engine.Evaluate(item);
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
