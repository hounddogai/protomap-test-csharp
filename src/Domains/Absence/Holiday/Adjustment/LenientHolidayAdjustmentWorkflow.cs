namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient holiday adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientHolidayAdjustmentWorkflow(ILenientHolidayAdjustmentStore Store, LenientHolidayAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientHolidayAdjustmentWorkflow NewLenientHolidayAdjustmentWorkflow(ILenientHolidayAdjustmentStore store, IClock clock)
    {
        return new LenientHolidayAdjustmentWorkflow(
            store,
            LenientHolidayAdjustmentRules.DefaultLenientHolidayAdjustmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient holiday adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientHolidayAdjustmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient holiday adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientHolidayAdjustmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
