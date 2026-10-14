namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandScheduleCoordinator(IPayBandScheduleStore Store, PayBandScheduleValidator Validator, IClock Clock, List<string> Journal, PayBandScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band schedule {code} not found");
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

/// <summary>Functions of pay band schedule workflow.</summary>
public static class PayBandScheduleWorkflow
{
    public static PayBandScheduleCoordinator NewPayBandScheduleCoordinator(IPayBandScheduleStore store, IClock clock)
    {
        return new PayBandScheduleCoordinator(
            store,
            PayBandScheduleRules.DefaultPayBandScheduleValidator(),
            clock,
            new List<string>(),
            PayBandScheduleEngine.DefaultPayBandScheduleEngine());
    }
}
