namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination schedule is in its life cycle.</summary>
public enum NominationScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A nomination schedule as the nomination team keeps it.</summary>
public sealed class NominationSchedule(string code, string name, List<string> tags, double threshold, string phone, NominationScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public string Phone { get; set; } = phone;
    public NominationScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a nomination schedule in status created.</summary>
    public static NominationSchedule NewNominationSchedule(string code, string name)
    {
        return new NominationSchedule(code, name, new List<string>(), 0.0, "", NominationScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationScheduleStatus next)
    {
        switch (Status)
        {
            case NominationScheduleStatus.Created:
                return next == NominationScheduleStatus.Published || next == NominationScheduleStatus.Retired;
            case NominationScheduleStatus.Validated:
                return next == NominationScheduleStatus.Published || next == NominationScheduleStatus.Retired;
            case NominationScheduleStatus.Published:
                switch (next)
                {
                    case NominationScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public NominationSchedule Copy()
    {
        return new NominationSchedule(Code, Name, Tags, Threshold, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
