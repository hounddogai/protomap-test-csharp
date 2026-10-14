namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal booking templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalBookingTemplateWorkflow(ISeasonalBookingTemplateStore Store, SeasonalBookingTemplateValidator Validator, IClock Clock, List<string> Journal, SeasonalBookingTemplateEngine Engine)
{
    public static SeasonalBookingTemplateWorkflow NewSeasonalBookingTemplateWorkflow(ISeasonalBookingTemplateStore store, IClock clock, Registry flags)
    {
        return new SeasonalBookingTemplateWorkflow(
            store,
            SeasonalBookingTemplateRules.DefaultSeasonalBookingTemplateValidator(),
            clock,
            new List<string>(),
            SeasonalBookingTemplateEngine.DefaultSeasonalBookingTemplateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal booking template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalBookingTemplateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal booking template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalBookingTemplateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal booking template {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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
