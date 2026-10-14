namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted booking variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedBookingVarianceWorkflow(IWeightedBookingVarianceStore Store, WeightedBookingVarianceValidator Validator, IClock Clock, List<string> Journal, WeightedBookingVarianceEngine Engine)
{
    public static WeightedBookingVarianceWorkflow NewWeightedBookingVarianceWorkflow(IWeightedBookingVarianceStore store, IClock clock)
    {
        return new WeightedBookingVarianceWorkflow(
            store,
            WeightedBookingVarianceRules.DefaultWeightedBookingVarianceValidator(),
            clock,
            new List<string>(),
            WeightedBookingVarianceEngine.DefaultWeightedBookingVarianceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBookingVarianceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBookingVarianceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking variance {code} not found");
        }
        var result = Engine.Run(item);
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
