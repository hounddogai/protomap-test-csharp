namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkReservationWorkflow(IBenchmarkReservationStore Store, BenchmarkReservationValidator Validator, IClock Clock, List<string> Journal, BenchmarkReservationEngine Engine)
{
    public static BenchmarkReservationWorkflow NewBenchmarkReservationWorkflow(IBenchmarkReservationStore store, IClock clock)
    {
        return new BenchmarkReservationWorkflow(
            store,
            BenchmarkReservationRules.DefaultBenchmarkReservationValidator(),
            clock,
            new List<string>(),
            BenchmarkReservationEngine.DefaultBenchmarkReservationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkReservationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkReservationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark reservation {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
