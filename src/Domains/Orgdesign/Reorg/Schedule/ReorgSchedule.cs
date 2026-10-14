namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg schedule is in its life cycle.</summary>
public enum ReorgScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A reorg schedule. Its code is unique within a tenant.</summary>
public sealed class ReorgSchedule(string code, string label, double amount, string phone, string notes, string locale, string summary, string location, ReorgScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public string Notes { get; set; } = notes;
    public string Locale { get; set; } = locale;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public ReorgScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a reorg schedule in status created.</summary>
    public static ReorgSchedule NewReorgSchedule(string code, string label)
    {
        return new ReorgSchedule(code, label, 0.0, "", "", "", "", "", ReorgScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgScheduleStatus next)
    {
        switch (Status)
        {
            case ReorgScheduleStatus.Created:
                return next == ReorgScheduleStatus.Published;
            case ReorgScheduleStatus.Validated:
                return next == ReorgScheduleStatus.Retired;
            case ReorgScheduleStatus.Published:
                switch (next)
                {
                    case ReorgScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ReorgSchedule Copy()
    {
        return new ReorgSchedule(Code, Label, Amount, Phone, Notes, Locale, Summary, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
