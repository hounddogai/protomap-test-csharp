namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination reservation is in its life cycle.</summary>
public enum NominationReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a nomination reservation and its current status.</summary>
public sealed class NominationReservation(string code, string label, string name, string location, string notes, Dictionary<string, string> attributes, string segment, int endsAt, int updatedAt, bool pinned, string skill, NominationReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Pinned { get; set; } = pinned;
    public string Skill { get; set; } = skill;
    public NominationReservationStatus Status { get; set; } = status;

    /// <summary>Starts a nomination reservation in status proposed.</summary>
    public static NominationReservation NewNominationReservation(string code, string label)
    {
        return new NominationReservation(
            code,
            label,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0,
            false,
            "",
            NominationReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationReservationStatus next)
    {
        switch (Status)
        {
            case NominationReservationStatus.Proposed:
                switch (next)
                {
                    case NominationReservationStatus.Approved:
                        return true;
                }
                return false;
            case NominationReservationStatus.Approved:
                switch (next)
                {
                    case NominationReservationStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationReservationStatus.Rejected:
                switch (next)
                {
                    case NominationReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round((double)EndsAt - (double)UpdatedAt, 2);
    }

    public NominationReservation Copy()
    {
        return new NominationReservation(
            Code,
            Label,
            Name,
            Location,
            Notes,
            Attributes,
            Segment,
            EndsAt,
            UpdatedAt,
            Pinned,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
