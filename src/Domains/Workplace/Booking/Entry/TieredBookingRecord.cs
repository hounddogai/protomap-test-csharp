namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered booking record is in its life cycle.</summary>
public enum TieredBookingRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered booking record, with what the planners need to act on it.</summary>
public sealed class TieredBookingRecord(string code, string name, double rate, string locale, double weight, string reviewer, int sequence, string grade, bool pinned, string contactEmail, string notes, TieredBookingRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public TieredBookingRecordStatus Status { get; set; } = status;

    /// <summary>Starts a tiered booking record in status created.</summary>
    public static TieredBookingRecord NewTieredBookingRecord(string code, string name)
    {
        return new TieredBookingRecord(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            false,
            "",
            "",
            TieredBookingRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBookingRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBookingRecordStatus next)
    {
        switch (Status)
        {
            case TieredBookingRecordStatus.Created:
                switch (next)
                {
                    case TieredBookingRecordStatus.Published:
                        return true;
                }
                return false;
            case TieredBookingRecordStatus.Validated:
                return next == TieredBookingRecordStatus.Published || next == TieredBookingRecordStatus.Retired;
            case TieredBookingRecordStatus.Published:
                switch (next)
                {
                    case TieredBookingRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBookingRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered booking record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Weight + (double)Sequence, 2);
    }

    public TieredBookingRecord Copy()
    {
        return new TieredBookingRecord(
            Code,
            Name,
            Rate,
            Locale,
            Weight,
            Reviewer,
            Sequence,
            Grade,
            Pinned,
            ContactEmail,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
