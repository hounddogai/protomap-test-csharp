namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted vendor schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedVendorScheduleCoordinator(IWeightedVendorScheduleStore Store, WeightedVendorScheduleValidator Validator, IClock Clock, List<string> Journal, WeightedVendorScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted vendor schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedVendorScheduleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted vendor schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedVendorScheduleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted vendor schedule {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of weighted vendor schedule workflow.</summary>
public static class WeightedVendorScheduleWorkflow
{
    public static WeightedVendorScheduleCoordinator NewWeightedVendorScheduleCoordinator(IWeightedVendorScheduleStore store, IClock clock, Registry flags)
    {
        return new WeightedVendorScheduleCoordinator(
            store,
            WeightedVendorScheduleRules.DefaultWeightedVendorScheduleValidator(),
            clock,
            new List<string>(),
            WeightedVendorScheduleEngine.DefaultWeightedVendorScheduleEngine(flags));
    }
}
