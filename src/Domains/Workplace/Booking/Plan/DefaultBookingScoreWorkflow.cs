namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default booking scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultBookingScoreWorkflow(IDefaultBookingScoreStore Store, DefaultBookingScoreValidator Validator, IClock Clock, List<string> Journal)
{
    public static DefaultBookingScoreWorkflow NewDefaultBookingScoreWorkflow(IDefaultBookingScoreStore store, IClock clock)
    {
        return new DefaultBookingScoreWorkflow(
            store,
            DefaultBookingScoreRules.DefaultDefaultBookingScoreValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default booking score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultBookingScoreStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default booking score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultBookingScoreStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
