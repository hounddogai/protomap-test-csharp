namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves holiday metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayMetricWorkflow(IHolidayMetricStore Store, HolidayMetricValidator Validator, IClock Clock, List<string> Journal, HolidayMetricEngine Engine)
{
    public static HolidayMetricWorkflow NewHolidayMetricWorkflow(IHolidayMetricStore store, IClock clock, Registry flags)
    {
        return new HolidayMetricWorkflow(
            store,
            HolidayMetricRules.DefaultHolidayMetricValidator(),
            clock,
            new List<string>(),
            HolidayMetricEngine.DefaultHolidayMetricEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayMetricStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayMetricStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday metric {code} not found");
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
